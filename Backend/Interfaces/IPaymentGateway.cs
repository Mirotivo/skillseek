public class PaymentResult
{
    public string PaymentId { get; set; }
    public string ApprovalUrl { get; set; } // For gateways like Stripe or PayPal
    public string Status { get; set; } // Optional: Add status information if needed
}

public interface IPaymentGateway
{
    Task<PaymentResult> CreatePayment(decimal amount, string currency, string returnUrl, string cancelUrl);
    Task<bool> CapturePayment(string paymentId);
    Task<string> RefundPayment(string paymentId, decimal amount);
    Task<bool> ProcessPayment(Transaction transaction);
}
