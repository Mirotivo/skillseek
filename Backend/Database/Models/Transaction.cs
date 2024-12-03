public class Transaction
{
    public int Id { get; set; }
    public int SenderId { get; set; } // Customer/User paying
    public User Sender { get; set; }
    public int? RecipientId { get; set; } // Nullable if paying the platform
    public User? Recipient { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; } // Deducted fee
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; } // e.g., "Pending", "Completed"
    public string PaymentMethod { get; set; } // e.g., "PayPal", "Card"
}
