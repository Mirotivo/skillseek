using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PurchaseOrder
{
    [Key]
    public int ID { get; set; }
    [MaxLength(255)]
    public string CustomerName { get; set; }
    [MaxLength(255)]
    public int StationGroupID { get; set; }
    [MaxLength(255)]
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    [ForeignKey("StationGroupID")]
    public StationGroup? StationGroup { get; set; }
    public string? Notes { get; set; }
    public PurchaseOrder()
    {
        CustomerName = string.Empty;
        Status = string.Empty;
        Source = string.Empty;
        OrderItems = new List<OrderItem>();
    }
}