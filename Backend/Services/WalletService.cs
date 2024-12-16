using Microsoft.EntityFrameworkCore;

public class WalletService : IWalletService
{
    private readonly skillseekDbContext _dbContext;
    private readonly PaymentGatewayFactory _paymentGatewayFactory;

    public WalletService(skillseekDbContext dbContext, PaymentGatewayFactory paymentGatewayFactory)
    {
        _dbContext = dbContext;
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    public async Task<(string PaymentId, string ApprovalUrl, int TransactionId)> AddMoneyToWallet(int userId, PaymentRequestDto request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            throw new InvalidOperationException("Wallet not found.");

        var paymentGateway = _paymentGatewayFactory.GetPaymentGateway("PayPal");

        // Create a payment
        var paymentResult = await paymentGateway.CreatePayment(request.Amount, "AUD", request.ReturnUrl, request.CancelUrl);
        if (string.IsNullOrEmpty(paymentResult.PaymentId) || string.IsNullOrEmpty(paymentResult.ApprovalUrl))
            throw new InvalidOperationException("Failed to create payment.");

        // Log the transaction as Pending
        var transaction = new Transaction
        {
            SenderId = userId,
            RecipientId = null, // Platform
            Amount = request.Amount,
            PlatformFee = 0,
            TransactionDate = DateTime.UtcNow,
            PaymentType = PaymentType.WalletTopUp,
            PaymentMethod = PaymentMethod.PayPal,
            Status = TransactionStatus.Pending,
            PaymentId = paymentResult.PaymentId
        };

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        return (paymentResult.PaymentId, paymentResult.ApprovalUrl, transaction.Id);
    }

    public async Task<bool> CapturePayment(int userId, string paymentId)
    {
        var paymentGateway = _paymentGatewayFactory.GetPaymentGateway("PayPal");

        // Capture the payment
        var success = await paymentGateway.CapturePayment(paymentId);
        if (!success)
            throw new InvalidOperationException("Failed to capture payment.");

        // Find the transaction
        var transaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.PaymentId == paymentId && t.SenderId == userId);
        if (transaction == null)
            throw new InvalidOperationException("Transaction not found.");

        // Update the transaction status
        transaction.Status = TransactionStatus.Completed;

        // Update the wallet balance
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            throw new InvalidOperationException("Wallet not found.");

        wallet.Balance += transaction.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<(decimal Balance, DateTime LastUpdated)> GetWalletBalance(int userId)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            throw new InvalidOperationException("Wallet not found.");

        return (wallet.Balance, wallet.UpdatedAt);
    }
}
