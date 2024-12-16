public class UserDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string? Address { get; set; }
    public string? SkypeId { get; set; }
    public string? HangoutId { get; set; }
    public string? RecommendationToken { get; set; }
    public List<string> ProfileVerified { get; set; }
    public bool PaymentDetailsAvailable { get; set; }

    // Analytics
    public int? LessonsCompleted { get; set; }
    public int? Evaluations { get; set; }
}
