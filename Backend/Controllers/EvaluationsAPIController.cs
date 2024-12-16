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
    private readonly IEvaluationService _evaluationService;
    private readonly ILogger<EvaluationsAPIController> _logger;

    public EvaluationsAPIController(
        IEvaluationService evaluationService,
        ILogger<EvaluationsAPIController> logger
    )
    {
        _evaluationService = evaluationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvaluationsAsync()
    {
        var userId = GetUserId();

        // Identify pending reviews based on the user's role
        var pendingReviews = await _evaluationService.GetPendingReviews(userId);
        var receivedReviews = await _evaluationService.GetReceivedReviews(userId);
        var sentReviews = await _evaluationService.GetSentReviews(userId);
        var recommendations = await _evaluationService.GetRecommendations(userId);

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
    public async Task<IActionResult> LeaveReviewAsync([FromBody] ReviewDto reviewDto)
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
        if (!await _evaluationService.LeaveReview(reviewDto, userId))
        {
            return BadRequest("You are not authorized to leave this review.");
        }

        return Ok(new
        {
            success = true,
            message = "Review submitted successfully."
        });
    }

    [HttpPost("recommendation")]
    public async Task<IActionResult> SubmitRecommendation([FromBody] ReviewDto dto)
    {
        var userId = GetUserId();

        if (dto.RevieweeId <= 0 || string.IsNullOrEmpty(dto.Feedback))
            return BadRequest("Invalid data.");

        if (!await _evaluationService.SubmitRecommendation(dto, userId))
        {
            return BadRequest("Failed to submit recommendation.");
        }

        return Ok(new
        {
            success = true,
            message = "Recommendation submitted successfully."
        });
    }
}

