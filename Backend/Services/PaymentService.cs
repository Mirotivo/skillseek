using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

public class PaymentService : IPaymentService
{
    private readonly skillseekDbContext _dbContext;
    private readonly PaymentGatewayFactory _paymentGatewayFactory;
    private readonly StripeOptions _stripeOptions;

    public PaymentService(
        skillseekDbContext dbContext,
        PaymentGatewayFactory paymentGatewayFactory,
        IOptions<StripeOptions> stripeOptions
    )
    {
        _dbContext = dbContext;
        _paymentGatewayFactory = paymentGatewayFactory;
        _stripeOptions = stripeOptions.Value;
    }

    public async Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request)
    {
        var gateway = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
        return await gateway.CreatePayment(request.Amount, request.Currency, request.ReturnUrl, request.CancelUrl);
    }

    public async Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request)
    {
        var gateway = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
        return await gateway.CapturePayment(request.PaymentId);
    }

    public async Task<bool> SaveCardAsync(int userId, SaveCardDto request)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrEmpty(user.StripeCustomerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = user.Email,
                Name = $"{user.FirstName} {user.LastName}",
            });

            user.StripeCustomerId = customer.Id;
            await _dbContext.SaveChangesAsync();
        }

        var cardService = new CardService();
        var card = await cardService.CreateAsync(user.StripeCustomerId, new CardCreateOptions
        {
            Source = request.StripeToken,
        });

        var userCard = new UserCard
        {
            UserId = userId,
            CardId = card.Id,
            Last4 = card.Last4,
            ExpMonth = card.ExpMonth,
            ExpYear = card.ExpYear,
            Type = request.Purpose,
        };

        _dbContext.UserCards.Add(userCard);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveCardAsync(int userId, int cardId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || string.IsNullOrEmpty(user.StripeCustomerId))
        {
            throw new KeyNotFoundException("User or Stripe customer not found.");
        }

        var card = await _dbContext.UserCards.FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId);
        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;
        var cardService = new CardService();
        await cardService.DeleteAsync(user.StripeCustomerId, card.CardId);

        _dbContext.UserCards.Remove(card);
        await _dbContext.SaveChangesAsync();

        // Check if there are any remaining cards for the user
        var remainingCards = _dbContext.UserCards.Any(c => c.UserId == userId);
        if (!remainingCards)
        {
            // If no cards remain, clear the StripeCustomerId
            user.StripeCustomerId = null;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        return true;
    }

    public async Task<IEnumerable<CardDto>> GetSavedCardsAsync(int userId)
    {
        return await _dbContext.UserCards
            .Where(c => c.UserId == userId)
            .Select(c => new CardDto
            {
                Id = c.Id,
                Last4 = c.Last4,
                ExpMonth = c.ExpMonth,
                ExpYear = c.ExpYear,
                Type = "Card", // Assuming "Card" is a default value for all cards
                Purpose = c.Type
            })
            .ToListAsync();
    }

    public async Task<PaymentHistoryDto> GetPaymentHistoryAsync(int userId)
    {
        // Fetch all relevant transactions involving the user
        var transactions = _dbContext.Transactions
            .Where(t => t.SenderId == userId || t.RecipientId == userId)
            .OrderByDescending(t => t.TransactionDate) // Order by date
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                SenderId = t.SenderId,
                SenderName = t.Sender.FirstName,
                RecipientId = t.RecipientId,
                RecipientName = t.Recipient != null ? t.Recipient.FirstName : "Platform",
                Amount = t.SenderId == userId ? -(t.Amount + t.PlatformFee) : (t.Amount + t.PlatformFee),
                PlatformFee = t.PlatformFee,
                Net = t.SenderId == userId
                    ? -t.Amount
                    : t.Amount,
                Status = t.Status.ToString(),
                TransactionDate = t.TransactionDate,
                Date = t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = t.RecipientId == null
                    ? $"Payment to Platform"
                    : t.SenderId == userId
                        ? $"Transferred to {t.Recipient.FirstName}"
                        : $"Received from {t.Sender.FirstName}",
                TransactionType = t.RecipientId == null
                    ? "To Platfothe rm"
                    : t.SenderId == userId
                        ? "Sent to User"
                        : "Received from User",
                Type = t.RecipientId == null
                    ? "transfer" // Transactions involving the platform are transfers
                    : "payment" // Transactions between users are payments
            })
            .ToList();

        // Filter transactions with RecipientId == null as invoices
        var invoices = transactions.Where(t => t.RecipientId == null).ToList();
        // Exclude invoices from the transactions list
        transactions = transactions.Where(t => t.RecipientId != null).ToList();

        // Fetch wallet balance
        var walletBalance = _dbContext.Wallets
            .Where(w => w.UserId == userId)
            .Select(w => w.Balance)
            .FirstOrDefault();

        // Calculate the total amount collected by the user
        var totalAmountCollected = _dbContext.Transactions
            .Where(t => t.RecipientId == userId && t.Status == TransactionStatus.Completed)
            .AsEnumerable()
            .Sum(t => t.Amount);

        // Response with chronological transaction history and balance
        return new PaymentHistoryDto
        {
            WalletBalance = walletBalance,
            TotalAmountCollected = totalAmountCollected,
            Invoices = invoices,
            Transactions = transactions
        };
    }

    public async Task AddPayPalAccountAsync(string userId, string payPalEmail)
    {
        if (string.IsNullOrEmpty(payPalEmail))
        {
            throw new ArgumentException("PayPal email is required.");
        }

        // Save the PayPal email to the user's account in the database
        var user = await _dbContext.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.PayPalAccountId = payPalEmail;
        await _dbContext.SaveChangesAsync();
    }


    public async Task ProcessPastBookedLessons()
    {
        var currentDate = DateTime.UtcNow;

        var pastBookedLessons = await _dbContext.Lessons
            .Include(l => l.Listing)
            .Include(l => l.Listing.User)
            .Include(l => l.Student)
            .Where(l => l.Status == LessonStatus.Booked && l.Date < currentDate)
            .ToListAsync();

        foreach (var lesson in pastBookedLessons)
        {
            lesson.Status = LessonStatus.Completed;
            await ProcessPaymentAsync(lesson);
        }

        if (pastBookedLessons.Any())
        {
            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task ProcessPaymentAsync(Lesson lesson)
    {
        var platformFeeRate = 0.1m;
        var platformFee = lesson.Price * platformFeeRate;
        var amountToTutor = lesson.Price - platformFee;

        var transaction = new Transaction
        {
            SenderId = lesson.StudentId,
            Sender = lesson.Student,
            RecipientId = lesson.Listing.UserId,
            Recipient = lesson.Listing.User,
            Amount = amountToTutor,
            PlatformFee = platformFee,
            TransactionDate = DateTime.UtcNow,
            PaymentType = PaymentType.Lesson,
            PaymentMethod = PaymentMethod.Card,
            Status = TransactionStatus.Pending,
        };

        await _dbContext.Transactions.AddAsync(transaction);

        var paymentGateway = _paymentGatewayFactory.GetPaymentGateway("Stripe");
        var paymentSuccessful = await paymentGateway.ProcessPayment(transaction);

        transaction.Status = paymentSuccessful ? TransactionStatus.Completed : TransactionStatus.Failed;

        if (paymentSuccessful)
        {
            await UpdateWalletBalance(transaction.RecipientId.Value, amountToTutor);
        }

        await _dbContext.SaveChangesAsync();
    }
    private async Task UpdateWalletBalance(int userId, decimal amount)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet != null)
        {
            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            _dbContext.Wallets.Update(wallet);
        }
        else
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = amount,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Wallets.AddAsync(wallet);
        }
    }

}