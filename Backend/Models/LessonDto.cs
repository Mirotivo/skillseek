public class LessonDto
{
    public int Id { get; set; }
    public string Topic { get; set; } // Always "Lesson" in this context
    public DateTime Date { get; set; } // The date of the lesson
    public TimeSpan Duration { get; set; }
    // public string Duration { get; set; } // e.g., "2 hours"
    public decimal Price { get; set; }
    public int TutorId { get; set; }
    public string Status { get; set; } // Status of the lesson (e.g., "Booked", "Completed", "Canceled")
}

public class CreateListingDto
{
    public string Title { get; set; }
    public string AboutLesson { get; set; }
    public string AboutYou { get; set; }
    public string Image { get; set; }
    public List<string> Locations { get; set; } // List of location names as strings
    public int LessonCategoryId { get; set; }
    public decimal HourRate { get; set; }
}
