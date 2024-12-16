using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

public class ListingService : IListingService
{
    private readonly skillseekDbContext _dbContext;

    public ListingService(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<ListingDto> GetDashboardListings()
    {
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .AsEnumerable()
            .OrderBy(_ => Guid.NewGuid())
            .Take(10)
            .ToList();

        return listings.Select(MapToListingDto);
    }

    public PagedResult<ListingDto> SearchListings(string query, int page, int pageSize)
    {
        var queryable = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => EF.Functions.Like(l.Title, $"%{query}%") ||
                        EF.Functions.Like(l.Description, $"%{query}%") ||
                        EF.Functions.Like(l.AboutLesson, $"%{query}%") ||
                        EF.Functions.Like(l.AboutYou, $"%{query}%"));

        var totalResults = queryable.Count();
        var results = queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToListingDto)
            .ToList();

        return new PagedResult<ListingDto>
        {
            TotalResults = totalResults,
            Page = page,
            PageSize = pageSize,
            Results = results
        };
    }

    public IEnumerable<ListingDto> GetUserListings(int userId)
    {
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.UserId == userId)
            .ToList();

        return listings.Select(MapToListingDto);
    }

    public ListingDto GetListingById(int id)
    {
        var listing = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .FirstOrDefault(l => l.Id == id);

        return listing == null ? null : MapToListingDto(listing);
    }

    public async Task<ListingDto> CreateListing(CreateListingWithImageDto createListingDto, int userId)
    {
        string? imageUrl = null;

        if (createListingDto.Image != null)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(SanitizeFileName(createListingDto.Image.FileName))}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await createListingDto.Image.CopyToAsync(stream);
            }

            imageUrl = $"https://localhost:9000/uploads/{uniqueFileName}";
        }

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

        await _dbContext.Listings.AddAsync(listing);
        await _dbContext.SaveChangesAsync();

        // Reload the listing with related data
        var loadedListing = await _dbContext.Listings
            .Include(l => l.User)
            .Include(l => l.LessonCategory)
            .Include(l => l.Rates)
            .FirstOrDefaultAsync(l => l.Id == listing.Id);

        if (loadedListing == null)
        {
            throw new InvalidOperationException("Failed to reload the listing after saving.");
        }

        return MapToListingDto(loadedListing);
    }


    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\.\-]", "_");

        // Ensure a unique file name by appending a GUID
        string extension = Path.GetExtension(fileName);
        string sanitizedBaseName = Path.GetFileNameWithoutExtension(fileName);
        string uniqueName = $"{sanitizedBaseName}_{Guid.NewGuid()}{extension}";

        return uniqueName;
    }

    private ListingDto MapToListingDto(Listing listing)
    {
        return new ListingDto
        {
            Id = listing.Id,
            TutorId = listing.User.Id,
            Name = listing.User?.FirstName,
            Category = listing.LessonCategory?.Name,
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
            ContactedCount = 5,
            SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
        };
    }
}
