using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    [Key]
    public int ID { get; set; }
    [MaxLength(255)]
    public string Url { get; set; }
    [MaxLength(255)]
    public string IconClass { get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    [MaxLength(255)]
    public string ImageUrl { get; set; }
    [Required]
    public int CategoryID { get; set; }
    [ForeignKey(nameof(Product.CategoryID))]
    public Category? Category { get; set; }
    [MaxLength(255)]
    public string Title { get; set; }
    [MaxLength(255)]
    public string Description { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public double Price { get; set; }
    public Product()
    {
        Url = string.Empty;
        IconClass = string.Empty;
        Name = string.Empty;
        ImageUrl = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
    }
}
