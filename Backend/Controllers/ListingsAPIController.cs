using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/listings")]
[ApiController]
public class ListingsAPIController : BaseController
{
    private readonly skillseekDbContext _dbContext;

    public ListingsAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("Dashboard")]
    public IActionResult Dashboard()
    {
        // Fetch all listings from the database
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .AsEnumerable()
            .OrderBy(_ => Guid.NewGuid()) // Randomize the order
            .Take(10) // Limit to 10 random listings (optional)
            .ToList(); // Execute SQL query and fetch data into memory

        var result = listings
            .Select(l => new ListingDto
            {
                Id = l.Id,
                TutorId = l.User.Id,
                Name = l.User.FirstName,
                ContactedCount = 5,
                Category = l.LessonCategory.Name,
                Title = l.Title,
                Image = l.Image,
                LessonsTaught = l.LessonCategory.Name,
                Locations = Enum.GetValues(typeof(LocationType))
                                .Cast<LocationType>()
                                .Where(location => (l.Locations & location) == location && location != LocationType.None)
                                .Select(location => location.ToString())
                                .ToList(),
                AboutLesson = l.AboutLesson,
                AboutYou = l.AboutYou,
                Rate = $"{l.Rates.Hourly}/h",
                Rates = new RatesDto
                {
                    Hourly = l.Rates.Hourly,
                    FiveHours = l.Rates.FiveHours,
                    TenHours = l.Rates.TenHours
                },
                SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
            })
            .ToList();

        if (result == null || !result.Any())
        {
            return NotFound("No listings found.");
        }

        return Ok(result);
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query cannot be empty.");
        }

        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and page size must be greater than 0.");
        }

        var queryable = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => EF.Functions.Like(l.Title, $"%{query}%") ||
                EF.Functions.Like(l.Description, $"%{query}%") ||
                EF.Functions.Like(l.AboutLesson, $"%{query}%") ||
                EF.Functions.Like(l.AboutYou, $"%{query}%")
            );

        var totalResults = queryable.Count();

        var results = queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListingDto
            {
                Id = l.Id,
                TutorId = l.User.Id,
                Name = l.User.FirstName,
                Category = l.LessonCategory.Name,
                Title = l.Title,
                Image = l.Image,
                AboutLesson = l.AboutLesson,
                AboutYou = l.AboutYou,
                Rate = $"{l.Rates.Hourly}/h",
                Rates = new RatesDto
                {
                    Hourly = l.Rates.Hourly,
                    FiveHours = l.Rates.FiveHours,
                    TenHours = l.Rates.TenHours
                }
            })
            .ToList();

        return Ok(new
        {
            TotalResults = totalResults,
            Page = page,
            PageSize = pageSize,
            Results = results
        });
    }


    [HttpGet()]
    public IActionResult Get()
    {
        var userId = GetUserId();

        // Fetch the data that can be directly translated to SQL
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.UserId == userId)
            .ToList(); // Execute SQL query and fetch data into memory

        var result = listings
            .Select(l => new ListingDto
            {
                Id = l.Id,
                Name = l.User.FirstName,
                Category = l.LessonCategory.Name,
                Title = l.Title,
                Image = l.Image,
                LessonsTaught = l.LessonCategory.Name,
                Locations = Enum.GetValues(typeof(LocationType))
                                .Cast<LocationType>()
                                .Where(location => (l.Locations & location) == location && location != LocationType.None)
                                .Select(location => location.ToString())
                                .ToList(),
                AboutLesson = l.AboutLesson,
                AboutYou = l.AboutYou,
                Rate = $"{l.Rates.Hourly}/h",
                Rates = new RatesDto
                {
                    Hourly = l.Rates.Hourly,
                    FiveHours = l.Rates.FiveHours,
                    TenHours = l.Rates.TenHours
                },
                SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
            })
            .ToList();
        return Ok(result);
    }


    [HttpGet("{id:int}")]
    public IActionResult GetListingById(int id)
    {
        var listing = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .FirstOrDefault(l => l.Id == id);

        if (listing == null)
        {
            return NotFound($"Listing with ID {id} not found.");
        }

        var result = new ListingDto
        {
            Id = listing.Id,
            Name = listing.User.FirstName,
            Category = listing.LessonCategory.Name,
            Title = listing.Title,
            Image = listing.Image,
            LessonsTaught = listing.LessonCategory.Name,
            Locations = Enum.GetValues(typeof(LocationType))
                            .Cast<LocationType>()
                            .Where(location => (listing.Locations & location) == location && location != LocationType.None)
                            .Select(location => location.ToString())
                            .ToList(),
            AboutLesson = listing.AboutLesson,
            AboutYou = listing.AboutYou,
            Rate = $"{listing.Rates.Hourly}/h",
            Rates = new RatesDto
            {
                Hourly = listing.Rates.Hourly,
                FiveHours = listing.Rates.FiveHours,
                TenHours = listing.Rates.TenHours
            },
            SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
        };

        return Ok(result);
    }


    [HttpPost("create-listing")]
    public async Task<IActionResult> Create([FromForm] CreateListingWithImageDto createListingDto)
    {
        if (createListingDto == null)
        {
            return BadRequest("Invalid data.");
        }

        var userId = GetUserId();
        var filePath = string.Empty;

        // Begin a transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            string? imageUrl = null;

            // Step 1: Upload the image (if provided)
            if (createListingDto.Image != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(createListingDto.Image.FileName)}";
                filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await createListingDto.Image.CopyToAsync(stream);
                }

                imageUrl = $"https://localhost:9000/uploads/{uniqueFileName}";
            }

            // Step 2: Create the listing
            var listing = new Listing
            {
                Title = createListingDto.Title,
                AboutLesson = createListingDto.AboutLesson,
                AboutYou = createListingDto.AboutYou,
                Image = imageUrl,
                Locations = createListingDto.Locations
                            .Aggregate(LocationType.None, (current, location) =>
                                current | Enum.Parse<LocationType>(location, true)),
                LessonCategoryId = createListingDto.LessonCategoryId,
                UserId = userId,
                Rates = new ListingRates
                {
                    Hourly = createListingDto.Rates.Hourly,
                    FiveHours = createListingDto.Rates.FiveHours,
                    TenHours = createListingDto.Rates.TenHours
                }
            };

            // Save to the database
            await _dbContext.Listings.AddAsync(listing);
            await _dbContext.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();

            // Return the created listing DTO
            return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, new ListingDto
            {
                Id = listing.Id,
                TutorId = userId,
                Name = listing.User?.FirstName,
                Category = listing.LessonCategory?.Name,
                Title = listing.Title,
                Image = listing.Image,
                LessonsTaught = listing.LessonCategory?.Name,
                Locations = Enum.GetValues(typeof(LocationType))
                                .Cast<LocationType>()
                                .Where(location => (listing.Locations & location) == location && location != LocationType.None)
                                .Select(location => location.ToString())
                                .ToList(),
                AboutLesson = listing.AboutLesson,
                AboutYou = listing.AboutYou,
                Rate = $"{listing.Rates.Hourly}/h",
                Rates = new RatesDto
                {
                    Hourly = listing.Rates.Hourly,
                    FiveHours = listing.Rates.FiveHours,
                    TenHours = listing.Rates.TenHours
                },
                SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
            });
        }
        catch (Exception ex)
        {
            // Rollback the transaction
            await transaction.RollbackAsync();

            // Delete the uploaded image if something fails
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the listing.");
        }
    }
}

