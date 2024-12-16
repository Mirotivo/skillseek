public interface IWalletService
{
    Task<(string PaymentId, string ApprovalUrl, int TransactionId)> AddMoneyToWallet(int userId, PaymentRequestDto request);
    Task<bool> CapturePayment(int userId, string paymentId);
    Task<(decimal Balance, DateTime LastUpdated)> GetWalletBalance(int userId);
}
