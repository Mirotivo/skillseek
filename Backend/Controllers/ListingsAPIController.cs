using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/listings")]
[ApiController]
public class ListingsAPIController : ControllerBase
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
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        // Parse the UserId
        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Invalid User ID.");
        }

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
}

