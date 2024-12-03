using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/subscriptions")]
[ApiController]
public class SubscriptionsAPIController : BaseController
{

    private readonly skillseekDbContext _dbContext;

    public SubscriptionsAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequestDto request)
    {
        var user = await _dbContext.Users.FindAsync(request.UserId);
        if (user == null)
            return NotFound("User not found.");

        // Log subscription payment as a transaction
        var transaction = new Transaction
        {
            SenderId = request.UserId,
            RecipientId = null, // Platform
            Amount = request.Amount,
            PlatformFee = 0, // No additional fee for subscriptions
            TransactionDate = DateTime.UtcNow,
            Status = "Pending",
            PaymentMethod = request.PaymentMethod
        };
        _dbContext.Transactions.Add(transaction);

        // Create the subscription
        var subscription = new Subscription
        {
            UserId = request.UserId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1), // 1-month subscription
            Amount = request.Amount,
            Status = "Active"
        };
        _dbContext.Subscriptions.Add(subscription);

        await _dbContext.SaveChangesAsync();

        return Ok(new { SubscriptionId = subscription.Id, TransactionId = transaction.Id });
    }

    [HttpGet("")]
    public async Task<IActionResult> GetUserSubscriptions()
    {
        var userId = GetUserId();

        var subscriptions = await _dbContext.Subscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (subscriptions == null || subscriptions.Count == 0)
            return NotFound("No subscriptions found.");

        return Ok(subscriptions);
    }

}

