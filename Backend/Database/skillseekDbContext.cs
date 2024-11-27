using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class skillseekDbContext : DbContext
{
    private readonly IConfiguration _config;

    public DbSet<User> Users { get; set; }
    public DbSet<Friendship> Friendships { get; set; }

    public DbSet<StationGroup> StationGroups { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public skillseekDbContext(DbContextOptions<skillseekDbContext> options, IConfiguration config)
        : base(options)
    {
        Users = Set<User>();
        Friendships = Set<Friendship>();
        _config = config;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Friendship>()
            .HasKey(f => new { f.UserId, f.FriendId });

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.User)
            .WithMany(u => u.Friends)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Friend)
            .WithMany(u => u.FriendOf)
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Restrict);
    }

}
