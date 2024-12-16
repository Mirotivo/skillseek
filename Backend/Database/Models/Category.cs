using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Category
{
    [Key]
    public int ID { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public Category()
    {
        Name = string.Empty;
    }
}
