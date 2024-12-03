public class SubscriptionRequestDto
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } // e.g., "PayPal"
}
