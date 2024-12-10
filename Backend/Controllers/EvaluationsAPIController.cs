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
    private readonly ILogger<EvaluationsAPIController> _logger;

    public EvaluationsAPIController(skillseekDbContext dbContext, ILogger<EvaluationsAPIController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetEvaluations()
    {
        var userId = GetUserId();

        // Identify pending reviews based on the user's role
        var pendingReviews = _dbContext.Lessons
            .Include(l => l.Listing)
            .Where(l => (l.Listing.UserId == userId || l.StudentId == userId) && l.Status == LessonStatus.Completed)
            .AsEnumerable()
            .SelectMany(l => new[]
            {
                // Pending reviews for students (written by tutor)
                new
                {
                    ReviewerId = userId,
                    RevieweeId = l.StudentId,
                    Role = "Student",
                    ReviewExists = _dbContext.Reviews.Any(r => r.ReviewerId == userId && r.RevieweeId == l.StudentId && r.Type == ReviewType.Review)
                },
                // Pending reviews for tutors (written by student)
                new
                {
                    ReviewerId = userId,
                    RevieweeId = l.Listing.UserId,
                    Role = "Tutor",
                    ReviewExists = _dbContext.Reviews.Any(r => r.ReviewerId == userId && r.RevieweeId == l.Listing.UserId && r.Type == ReviewType.Review)
                }
            })
            .Where(x => !x.ReviewExists) // Only include those without a review
            .Where(x => x.RevieweeId != userId) // Exclude cases where the user is reviewing themselves
            .Distinct()
            .ToList()
            .Select(x =>
            {
                var reviewee = _dbContext.Users.FirstOrDefault(u => u.Id == x.RevieweeId);
                return new ReviewDto
                {
                    RevieweeId = x.RevieweeId,
                    Name = reviewee?.FirstName ?? reviewee?.Email,
                    Subject = $"Pending Review for {x.Role}",
                    Feedback = $"You have not reviewed this {x.Role.ToLower()} yet.",
                    Avatar = reviewee?.ProfileImage
                };
            })
            .ToList();


        var receivedReviews = _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Review && r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Select(r => new ReviewDto
            {
                RevieweeId = r.RevieweeId,
                Name = r.Reviewer.FirstName ?? r.Reviewer.Email,
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = r.Reviewer.ProfileImage
            })
            .ToList();

        var sentReviews = _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Review && r.ReviewerId == userId)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Select(r => new ReviewDto
            {
                RevieweeId = r.RevieweeId,
                Name = r.Reviewee.FirstName ?? r.Reviewee.Email,
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = r.Reviewee.ProfileImage
            })
            .ToList();

        var recommendations = _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Recommendation && r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .Select(r => new ReviewDto
            {
                RevieweeId = r.RevieweeId,
                Name = r.Reviewee.FirstName ?? r.Reviewee.Email,
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = r.Reviewee.ProfileImage
            })
            .ToList();


        if (!pendingReviews.Any() && !receivedReviews.Any() && !sentReviews.Any() && !recommendations.Any())
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

    [HttpPost("review")]
    public IActionResult LeaveReview([FromBody] ReviewDto reviewDto)
    {
        var userId = GetUserId();

        // Validate the input
        if (reviewDto == null)
        {
            return BadRequest("Review data is required.");
        }

        if (reviewDto.RevieweeId <= 0 || string.IsNullOrWhiteSpace(reviewDto.Subject) || string.IsNullOrWhiteSpace(reviewDto.Feedback))
        {
            return BadRequest("Invalid review details.");
        }

        // Check if the user is authorized to write this review (optional logic)
        var lessonExists = _dbContext.Lessons.Any(l =>
            (l.StudentId == reviewDto.RevieweeId &&
            l.Listing.UserId == userId &&
            l.Status == LessonStatus.Completed)
            ||
            (l.StudentId == userId &&
            l.Listing.UserId == reviewDto.RevieweeId &&
            l.Status == LessonStatus.Completed));

        if (!lessonExists)
        {
            return Forbid("You cannot write a review for this student.");
        }

        // Create the review entity
        var review = new Review
        {
            ReviewerId = userId,
            RevieweeId = reviewDto.RevieweeId,
            Type = ReviewType.Review, // Assuming "Review" is the type for standard reviews
            Title = reviewDto.Subject,
            Comments = reviewDto.Feedback,
            CreatedAt = DateTime.UtcNow
        };

        // Save the review
        _dbContext.Reviews.Add(review);
        _dbContext.SaveChanges();

        return Ok(new
        {
            success = true,
            message = "Review submitted successfully."
        });
    }
}

