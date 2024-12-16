using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

[Flags]
public enum Role
{
    None = 0,
    Student = 1, // 01 in binary
    Tutor = 2    // 10 in binary
}

public class User : IAccountable
{
    [Key]
    public int Id { get; set; }
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(255)]
    public string Email { get; set; }
    [MaxLength(255)]
    public string PasswordHash { get; set; }
    [MaxLength(15)]
    public string? Phone { get; set; }
    [MaxLength(255)]
    public string? Address { get; set; }
    [MaxLength(255)]
    public string? PayPalAccountId { get; set; } // For payouts
    [MaxLength(255)]
    public string? StripeCustomerId { get; set; } // For payouts
    [MaxLength(255)]
    public string? SkypeId { get; set; }
    [MaxLength(255)]
    public string? HangoutId { get; set; }
    [MaxLength(255)]
    public string? ProfileImage { get; set; }
    [MaxLength(8)]
    public string? RecommendationToken { get; set; }

    public List<Friendship> Friends { get; set; }
    public List<Friendship> FriendOf { get; set; }
    public Role Roles { get; set; }
    public List<UserCard> UserCards { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Friends = new List<Friendship>();
        FriendOf = new List<Friendship>();
        UserCards = new List<UserCard>();
    }

    public override string ToString()
    {
        return $"User: {Id}, Name: {FirstName} {LastName}, Email: {Email}, Active: {Active}, CreatedAt: {CreatedAt}";
    }
}
