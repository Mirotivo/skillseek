public class ReviewDto
{
    public string Name { get; set; }
    public string Subject { get; set; }
    public string? Message { get; set; } // For pending reviews
    public string? Feedback { get; set; } // For received or sent reviews
    public string? Avatar { get; set; } // Optional avatar URL
}
