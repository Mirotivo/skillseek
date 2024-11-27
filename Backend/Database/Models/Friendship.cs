public class Friendship
{
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public User User { get; set; }
    public User Friend { get; set; }

    public Friendship()
    {
        UserId = 0;
        FriendId = 0;
    }
}
