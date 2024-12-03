public class Payout
{
    public int Id { get; set; }
    public int UserId { get; set; } // Recipient of the payout
    public User User { get; set; }
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; } // Nullable if not processed
    public string Status { get; set; } // e.g., "Pending", "Completed"
}
