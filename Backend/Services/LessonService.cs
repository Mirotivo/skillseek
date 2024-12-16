using Microsoft.EntityFrameworkCore;

public class LessonService : ILessonService
{
    private readonly skillseekDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly PaymentGatewayFactory _paymentGatewayFactory;

    public LessonService(
        skillseekDbContext dbContext,
        INotificationService notificationService,
        PaymentGatewayFactory paymentGatewayFactory
    )
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    public async Task<LessonDto> ProposeLesson(LessonDto lessonDto, int userId)
    {
        if (lessonDto.StudentId == null || lessonDto.StudentId == 0)
        {
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

        await _dbContext.Lessons.AddAsync(lesson);
        await _dbContext.SaveChangesAsync();

        // Notify the tutor
        var student = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstOrDefaultAsync();

        var tutorId = await _dbContext.Listings
            .Where(l => l.Id == lessonDto.ListingId)
            .Select(l => l.UserId)
            .FirstOrDefaultAsync();

        if (tutorId != 0)
        {
            var studentName = $"{student?.FirstName} {student?.LastName}".Trim();

            await _notificationService.SendNotificationAsync(
                tutorId.ToString(),
                NotificationEvent.ChatRequest,
                $"{studentName} has proposed a new lesson.",
                new
                {
                    LessonId = lesson.Id,
                    Date = lesson.Date,
                    Duration = lesson.Duration,
                    Price = lesson.Price
                }
            );
        }

        return new LessonDto
        {
            Id = lesson.Id,
            Date = lesson.Date,
            Duration = lesson.Duration,
            Price = lesson.Price,
            Status = lesson.Status
        };
    }

    public async Task<bool> RespondToProposition(int lessonId, bool accept, int userId)
    {
        var lesson = await _dbContext.Lessons
            .Include(l => l.Listing)
            .ThenInclude(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
        {
            return false;
        }

        lesson.Status = accept ? LessonStatus.Booked : LessonStatus.Canceled;
        await _dbContext.SaveChangesAsync();

        // Notify the student
        var studentId = lesson.StudentId;

        if (studentId != 0)
        {
            var tutorName = $"{lesson.Listing.User.FirstName} {lesson.Listing.User.LastName}".Trim();
            var lessonTitle = lesson.Listing.Title ?? "the lesson";

            var statusMessage = accept
                ? $"Your proposition for {lessonTitle} has been accepted by {tutorName}."
                : $"Your proposition for {lessonTitle} has been declined by {tutorName}.";

            await _notificationService.SendNotificationAsync(
                studentId.ToString(),
                NotificationEvent.SystemAlert,
                statusMessage,
                new
                {
                    LessonId = lesson.Id,
                    Status = lesson.Status
                }
            );
        }

        return true;
    }

    public async Task<List<LessonDto>> GetPropositionsAsync(int contactId, int userId)
    {
        return await _dbContext.Lessons
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
                Status = p.Status
            })
            .ToListAsync();
    }

    public async Task<List<LessonDto>> GetLessonsAsync(int contactId, int userId)
    {
        return await _dbContext.Lessons
            .Where(p => (p.Status == LessonStatus.Booked || p.Status == LessonStatus.Completed || p.Status == LessonStatus.Canceled))
            .Where(p => p.StudentId == contactId)
            .OrderByDescending(p => p.Date)
            .Select(p => new LessonDto
            {
                Id = p.Id,
                Topic = "Lesson",
                Date = p.Date,
                Duration = p.Duration,
                Status = p.Status
            })
            .ToListAsync();
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
