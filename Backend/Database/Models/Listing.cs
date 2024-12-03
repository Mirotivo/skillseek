using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Flags]
public enum LocationType
{
    None = 0,          // 0000
    Webcam = 1 << 1,   // 0010
    TutorLocation = 1 << 2, // 0100
    StudentLocation = 1 << 2, // 1000
}

public class Listing : IOwnableAccountable
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(Listing.UserId))]
    public User User { get; set; }

    public int LessonCategoryId { get; set; }

    [ForeignKey(nameof(Listing.LessonCategoryId))]
    public LessonCategory LessonCategory { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public decimal HourRate { get; set; }

    public string Image { get; set; }

    public LocationType Locations { get; set; }

    public string AboutYou { get; set; }

    public string AboutLesson { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool Active { get ; set; }
    public DateTime CreatedAt { get ; set; }
    public DateTime UpdatedAt { get ; set; }
    public DateTime? DeletedAt { get ; set; }

    public Listing()
    {
        Description = string.Empty;
    }
}
