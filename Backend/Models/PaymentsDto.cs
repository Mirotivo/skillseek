public class PaymentRequestDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
}

public class AddMoneyRequestDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } // e.g., "PayPal"
}

public class PaymentHistoryDto
{
    public decimal WalletBalance { get; set; }
    public decimal TotalAmountCollected { get; set; }
    public List<TransactionDto> Invoices { get; set; }
    public List<TransactionDto> Transactions { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; }
    public int? RecipientId { get; set; }
    public string RecipientName { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal Net { get; set; }
    public string Status { get; set; }
    public string TransactionType { get; set; }
    public string Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Date { get; set; }
    public string Type { get; set; }
}

public class AddPayPalAccountDto
{
    public string PayPalEmail { get; set; }
}

public class CreateStripeSessionDto
{
    public int ListingId { get; set; }
    public decimal Price { get; set; }
}

public class SaveCardDto
{
    public string StripeToken { get; set; }
    public string Purpose { get; set; }
}

public class CardDto
{
    public int Id { get; set; }
    public string Last4 { get; set; }
    public long ExpMonth { get; set; }
    public long ExpYear { get; set; }
    public string Type { get; set; }
    public string Purpose { get; set; }
}