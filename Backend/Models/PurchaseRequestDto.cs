public class PurchaseRequestDto
{
    public int UserId { get; set; }
    public int RecipientId { get; set; } // Seller or recipient
    public decimal Amount { get; set; }
}
