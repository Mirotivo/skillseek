using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrderItem
{
    [Key]
    public int ID { get; set; }
    [Required]
    public int CategoryID { get; set; }
    [Required]
    public int PurchaseOrderID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    [ForeignKey("CategoryID")]
    public Category Category { get; set; }
    [ForeignKey("PurchaseOrderID")]
    public PurchaseOrder PurchaseOrder { get; set; }
}