using System.ComponentModel;

[Flags]
public enum Role
{
    None = 0,
    Student = 1, // 01 in binary
    Tutor = 2    // 10 in binary
}

public class User : IAccountable
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? PayPalAccountId { get; set; } // For payouts
    public string? SkypeId { get; set; }
    public string? HangoutId { get; set; }
    public string? ProfileImage { get; set; }
    public List<Friendship> Friends { get; set; }
    public List<Friendship> FriendOf { get; set; }
    public Role Roles { get; set; }

    public bool Active { get ; set; }
    public DateTime CreatedAt { get ; set; }
    public DateTime UpdatedAt { get ; set; }
    public DateTime? DeletedAt { get ; set; }

    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Friends = new List<Friendship>();
        FriendOf = new List<Friendship>();
    }

    public override string ToString()
    {
        return $"{Id} - {FirstName} {LastName} - {Email}";
    }
}
