public class Wallet : IUpdatable
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public decimal Balance { get; set; }
    public DateTime UpdatedAt { get; set; }
}
