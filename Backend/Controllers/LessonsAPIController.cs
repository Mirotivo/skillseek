using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using skillseek.Models;
using Stripe;

namespace skillseek.Controllers;

[Route("api/lessons")]
[ApiController]
public class LessonsAPIController : BaseController
{
    private readonly StripeOptions _stripeOptions;
    private readonly skillseekDbContext _dbContext;
    private readonly ILogger<LessonsAPIController> _logger;

    public LessonsAPIController(skillseekDbContext dbContext, IOptions<StripeOptions> stripeOptions, ILogger<LessonsAPIController> logger)
    {
        _dbContext = dbContext;
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
    }

    [HttpPost("proposeLesson")]
    public IActionResult ProposeLesson([FromBody] LessonDto lessonDto)
    {
        var userId = GetUserId();

        if (lessonDto.StudentId == null || lessonDto.StudentId == 0)
        {
            // If StudentId is not provided in lessonDto, Initiated by student
            lessonDto.StudentId = userId;
        }

        var lesson = new Lesson
        {
            Date = lessonDto.Date,
            Duration = lessonDto.Duration,
            Price = lessonDto.Price,
            StudentId = lessonDto.StudentId.Value,
            ListingId = lessonDto.ListingId,
            IsStudentInitiated = lessonDto.StudentId == userId,
            Status = LessonStatus.Proposed
        };

        _dbContext.Lessons.Add(lesson);
        _dbContext.SaveChanges();

        return Ok(new { Message = "Lesson proposed successfully.", LessonId = lesson.Id });
    }

    [HttpPost("respondToProposition/{lessonId}")]
    public IActionResult RespondToProposition(int lessonId, [FromBody] bool accept)
    {
        var userId = GetUserId();

        // Fetch the proposition
        var lesson = _dbContext.Lessons
            .Include(l => l.Listing)
            .FirstOrDefault(l => l.Id == lessonId);

        if (lesson == null)
        {
            return NotFound(new { Message = "Lesson not found." });
        }

        // Update the status based on the response
        lesson.Status = accept ? LessonStatus.Booked : LessonStatus.Canceled;

        // Save changes
        _dbContext.SaveChanges();

        return Ok(new
        {
            Message = accept ? "Proposition accepted successfully." : "Proposition refused successfully.",
            LessonId = lesson.Id,
            Status = lesson.Status.ToString()
        });
    }

    [HttpGet("{contactId}")]
    public IActionResult GetLessons(int contactId)
    {
        var userId = GetUserId();

        // Update the status of past Booked lessons to Completed
        var currentDate = DateTime.UtcNow;
        var pastBookedLessons = _dbContext.Lessons
            .Include(l => l.Listing)
            .Include(l => l.Listing.User)
            .Include(l => l.Student)
            .Where(l => l.Status == LessonStatus.Booked && l.Date < currentDate)
            .ToList();
        foreach (var lesson in pastBookedLessons)
        {
            lesson.Status = LessonStatus.Completed;
            // Process payment for completed lessons
            ProcessPayment(lesson);
        }
        if (pastBookedLessons.Any())
        {
            _dbContext.SaveChanges();
        }

        // Fetch Propositions
        var propositions = _dbContext.Lessons
            .Where(p => p.Status == LessonStatus.Proposed)
            .Where(p => p.IsStudentInitiated && userId == p.Listing.UserId)
            .Where(p => p.StudentId == contactId)
            .OrderByDescending(p => p.Date)
            .Select(p => new LessonDto
            {
                Id = p.Id,
                Date = p.Date,
                Duration = p.Duration,
                Price = p.Price,
                Status = p.Status.ToString()
            })
            .ToList();

        // Fetch Lessons
        var lessons = _dbContext.Lessons
            .Where(p => p.Status == LessonStatus.Booked || p.Status == LessonStatus.Completed || p.Status == LessonStatus.Canceled)
            .Where(l => l.StudentId == contactId)
            .OrderByDescending(p => p.Date)
            .Select(l => new LessonDto
            {
                Id = l.Id,
                Topic = "Lesson",
                Date = l.Date,
                Duration = l.Duration,
                Status = l.Status.ToString()
            })
            .ToList();

        return Ok(new
        {
            Propositions = propositions,
            Lessons = lessons
        });
    }

    private void ProcessPayment(Lesson lesson)
    {
        // Calculate the platform fee (e.g., 10% of the lesson price)
        var platformFeeRate = 0.1m;
        var platformFee = lesson.Price * platformFeeRate;
        var amountToTutor = lesson.Price - platformFee;

        // Create a new transaction
        var transaction = new Transaction
        {
            SenderId = lesson.StudentId,
            Sender = lesson.Student,
            RecipientId = lesson.Listing.UserId,
            Recipient = lesson.Listing.User,
            Amount = amountToTutor,
            PlatformFee = platformFee,
            TransactionDate = DateTime.UtcNow,
            Status = TransactionStatus.Pending,
            PaymentMethod = PaymentMethod.Card
        };

        _dbContext.Transactions.Add(transaction);

        // Process payment (e.g., integrate with payment gateway)
        var paymentSuccessful = ProcessPaymentGateway(transaction);

        // Update transaction status based on payment success
        transaction.Status = paymentSuccessful ? TransactionStatus.Completed : TransactionStatus.Failed;

        if (paymentSuccessful)
        {
            // Update the tutor's wallet balance
            UpdateWalletBalance(transaction.RecipientId.Value, amountToTutor);
        }

        _dbContext.SaveChanges();
    }

    private void UpdateWalletBalance(int userId, decimal amount)
    {
        var wallet = _dbContext.Wallets.FirstOrDefault(w => w.UserId == userId);
        if (wallet != null)
        {
            // Update existing wallet balance
            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            _dbContext.Wallets.Update(wallet);
        }
        else
        {
            // Create a new wallet if none exists
            wallet = new Wallet
            {
                UserId = userId,
                Balance = amount,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Wallets.Add(wallet);
        }
    }

    private bool ProcessPaymentGateway(Transaction transaction)
    {
        try
        {
            // Initialize Stripe API key
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

            // Create the charge options
            var options = new ChargeCreateOptions
            {
                Amount = (long)((transaction.Amount + transaction.PlatformFee) * 100),
                Currency = "AUD",
                Customer = transaction.Sender.StripeCustomerId.ToString(),
                Description = $"Payment for Lesson ID {transaction.Id}",
            };

            // Create the charge
            var service = new ChargeService();
            var charge = service.Create(options);

            // Check if the payment was successful
            if (charge.Status == "succeeded")
            {
                return true;
            }

            // Log failure details
            _logger.LogError($"Payment failed: {charge.FailureMessage}");
            return false;
        }
        catch (Exception ex)
        {
            // Log exception details
            _logger.LogError(ex, "Error processing payment through Stripe.");
            return false;
        }
    }

}

