using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class StationGroup
{
    [Key]
    public int ID { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }
}
