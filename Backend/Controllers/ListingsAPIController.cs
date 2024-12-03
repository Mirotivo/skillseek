using System.Diagnostics;
using System.Security.Claims;
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
                Rate = $"{l.HourRate}/h",
                Rates = new RatesDto
                {
                    Hourly = $"{l.HourRate}/h",
                    FiveHours = $"{l.HourRate * 5}/h",
                    TenHours = $"{l.HourRate * 10}/h"
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

    [HttpGet()]
    public IActionResult Get()
    {
        var userId = GetUserId();

        // Fetch the data that can be directly translated to SQL
        var listings = _dbContext.Listings
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
                Rate = $"{l.HourRate}/h",
                Rates = new RatesDto
                {
                    Hourly = $"{l.HourRate}/h",
                    FiveHours = $"{l.HourRate * 5}/h",
                    TenHours = $"{l.HourRate * 10}/h"
                },
                SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
            })
            .ToList();

        if (result == null || !result.Any())
        {
            return NotFound("No listings found for this user.");
        }

        return Ok(result);
    }

    [HttpPost("create-listing")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListingDto createListingDto)
    {
        if (createListingDto == null)
        {
            return BadRequest("Invalid data.");
        }

        var userId = GetUserId();

        // Validate and map the DTO to a Listing entity
        var listing = new Listing
        {
            Title = createListingDto.Title,
            AboutLesson = createListingDto.AboutLesson,
            AboutYou = createListingDto.AboutYou,
            Image = createListingDto.Image,
            Locations = createListingDto.Locations
                        .Aggregate(LocationType.None, (current, location) =>
                            current | Enum.Parse<LocationType>(location, true)),
            LessonCategoryId = createListingDto.LessonCategoryId,
            UserId = userId,
            HourRate = createListingDto.HourRate
        };

        // Save to the database
        await _dbContext.Listings.AddAsync(listing);
        await _dbContext.SaveChangesAsync();

        // Map the saved entity to a DTO for the response
        var result = new ListingDto
        {
            Id = listing.Id,
            TutorId = userId,
            Name = listing.User?.FirstName, // Assuming `User` is eager-loaded
            Category = listing.LessonCategory?.Name, // Assuming `LessonCategory` is eager-loaded
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
            Rate = $"{listing.HourRate}/h",
            Rates = new RatesDto
            {
                Hourly = $"{listing.HourRate}/h",
                FiveHours = $"{listing.HourRate * 5}/h",
                TenHours = $"{listing.HourRate * 10}/h"
            },
            SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
        };

        return CreatedAtAction(nameof(Get), new { id = listing.Id }, result);
    }

}

