
public interface IPaymentService
{
    Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request);
    Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request);
    Task<PaymentHistoryDto> GetPaymentHistoryAsync(int userId);

    Task<bool> SaveCardAsync(int userId, SaveCardDto request);
    Task<bool> RemoveCardAsync(int userId, int cardId);
    Task<IEnumerable<CardDto>> GetSavedCardsAsync(int userId);

    Task AddPayPalAccountAsync(string userId, string payPalEmail);
}
