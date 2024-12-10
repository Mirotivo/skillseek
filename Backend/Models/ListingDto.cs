public class ListingDto
{
    public int Id { get; set; }
    public int TutorId { get; set; }
    public string Name { get; set; }
    public int ContactedCount { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }
    public string LessonsTaught { get; set; }
    public List<string> Locations { get; set; }
    public string AboutLesson { get; set; }
    public string AboutYou { get; set; }
    public string Rate { get; set; }
    public RatesDto Rates { get; set; }
    public List<string> SocialPlatforms { get; set; }
}

public class RatesDto
{
    public decimal Hourly { get; set; }
    public decimal FiveHours { get; set; }
    public decimal TenHours { get; set; }
}

public class CreateListingDto
{
    public string Title { get; set; }
    public string AboutLesson { get; set; }
    public string AboutYou { get; set; }
    public string Image { get; set; }
    public List<string> Locations { get; set; } // List of location names as strings
    public int LessonCategoryId { get; set; }
    public RatesDto Rates { get; set; }
    public decimal HourRate { get; set; }
}

public class CreateListingWithImageDto
{
    public IFormFile? Image { get; set; } // The uploaded image file

    public string Title { get; set; }
    public string AboutLesson { get; set; }
    public string AboutYou { get; set; }
    public List<string> Locations { get; set; } // List of location names as strings
    public int LessonCategoryId { get; set; }
    public RatesDto Rates { get; set; }
    public decimal HourRate { get; set; }
}
