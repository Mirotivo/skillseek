using System.ComponentModel;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? SkypeId { get; set; }
    public string? HangoutId { get; set; }
    public string? ProfileImage { get; set; }
    public List<Friendship> Friends { get; set; }
    public List<Friendship> FriendOf { get; set; }

    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Friends = new List<Friendship>();
        FriendOf = new List<Friendship>();
    }

}
