using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LessonCategory
{
    [Key]
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }
}
