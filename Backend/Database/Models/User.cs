public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
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
