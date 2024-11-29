using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/evaluations")]
[ApiController]
public class EvaluationsAPIController : ControllerBase
{
    private readonly skillseekDbContext _dbContext;

    public EvaluationsAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
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

        // Fetch reviews and group them
        var pendingReviews = new List<ReviewDto>
        {
            new ReviewDto
            {
                Name = "Aziz",
                Subject = "Maths student",
                Message = "How did your lesson go?",
                Avatar = null
            },
            new ReviewDto
            {
                Name = "Alice",
                Subject = "Maths student",
                Message = "How did your lesson go?",
                Avatar = null
            },
            new ReviewDto
            {
                Name = "Eloise",
                Subject = "Maths student",
                Message = "How did your lesson go?",
                Avatar = null
            }
        };

        var receivedReviews = _dbContext.Reviews
            .Where(r => r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Select(r => new ReviewDto
            {
                Name = r.Reviewer.Email ?? "Unknown",
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = null
            })
            .ToList();

        var sentReviews = _dbContext.Reviews
            .Where(r => r.ReviewerId == userId)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Select(r => new ReviewDto
            {
                Name = r.Reviewee.Email ?? "Unknown",
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = null
            })
            .ToList();

        if (!pendingReviews.Any() && !receivedReviews.Any() && !sentReviews.Any())
        {
            return NotFound("No evaluations found for this user.");
        }

        return Ok(new
        {
            PendingReviews = pendingReviews,
            ReceivedReviews = receivedReviews,
            SentReviews = sentReviews
        });
    }
}

