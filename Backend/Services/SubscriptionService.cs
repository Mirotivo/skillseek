using Microsoft.EntityFrameworkCore;

public class SubscriptionService : ISubscriptionService
{
    private readonly skillseekDbContext _dbContext;

    public SubscriptionService(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CheckActiveSubscription(int userId)
    {
        return await _dbContext.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow);
    }

    public async Task<(int SubscriptionId, int TransactionId)> CreateSubscription(SubscriptionRequestDto request, int userId)
    {
        PaymentMethod paymentMethod = PaymentMethod.Stripe;
        if (!string.IsNullOrEmpty(request.PaymentMethod) &&
            !Enum.TryParse(request.PaymentMethod, true, out paymentMethod))
        {
            paymentMethod = PaymentMethod.Stripe;
        }

        // Log subscription payment as a transaction
        var transaction = new Transaction
        {
            SenderId = userId,
            RecipientId = null, // Platform
            Amount = request.Amount,
            PlatformFee = 0, // No additional fee for subscriptions
            TransactionDate = DateTime.UtcNow,
            PaymentType = PaymentType.StudentMembership,
            PaymentMethod = paymentMethod,
            Status = TransactionStatus.Completed,
        };
        await _dbContext.Transactions.AddAsync(transaction);

        // Create the subscription
        var subscription = new Subscription
        {
            UserId = userId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1), // 1-month subscription
            Amount = request.Amount,
            Status = SubscriptionStatus.Active,
            Plan = "Student Pass"
        };
        await _dbContext.Subscriptions.AddAsync(subscription);

        await _dbContext.SaveChangesAsync();

        return (subscription.Id, transaction.Id);
    }

    public async Task<List<Subscription>> GetUserSubscriptions(int userId)
    {
        return await _dbContext.Subscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }
}
