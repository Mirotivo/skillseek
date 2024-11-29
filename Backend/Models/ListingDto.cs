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
    public string Hourly { get; set; }
    public string FiveHours { get; set; }
    public string TenHours { get; set; }
}
