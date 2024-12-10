using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed
}

public enum PaymentMethod
{
    PayPal,
    Card,
    Wallet
}

public class Transaction
{
    [Key]
    public int Id { get; set; }
    public int SenderId { get; set; }
    [ForeignKey(nameof(Transaction.SenderId))]
    public User? Sender { get; set; }
    public int? RecipientId { get; set; } // Nullable if paying the platform
    [ForeignKey(nameof(Transaction.RecipientId))]
    public User? Recipient { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Transaction()
    {

    }
    public override string ToString()
    {
        return $"Transaction: {Id}, SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount:C}, Status: {Status}, PaymentMethod: {PaymentMethod}, Date: {TransactionDate}";
    }
}
