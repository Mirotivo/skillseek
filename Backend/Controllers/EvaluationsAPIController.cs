using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/evaluations")]
[ApiController]
public class EvaluationsAPIController : BaseController
{
    private readonly skillseekDbContext _dbContext;

    public EvaluationsAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet()]
    public IActionResult Get()
    {
        var userId = GetUserId();

        // Fetch reviews and group them
        var pendingReviews = new List<ReviewDto>{};

        var receivedReviews = _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Review && r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Select(r => new ReviewDto
            {
                Name = r.Reviewer.FirstName ?? "Unknown",
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = null
            })
            .ToList();

        var sentReviews = _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Review && r.ReviewerId == userId)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Select(r => new ReviewDto
            {
                Name = r.Reviewee.FirstName ?? "Unknown",
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = null
            })
            .ToList();

        var recommendations = _dbContext.Reviews
            .Where(r =>r.Type == ReviewType.Recommendation && r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .Select(r => new ReviewDto
            {
                Name = r.Reviewee.FirstName ?? "Unknown",
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
            SentReviews = sentReviews,
            Recommendations = recommendations
        });
    }
}

