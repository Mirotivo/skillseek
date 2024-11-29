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


    public DbSet<LessonCategory> LessonCategories { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

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


        // Review configuration
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewee)
            .WithMany()
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Chat configuration
        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Student)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Tutor)
            .WithMany()
            .HasForeignKey(c => c.TutorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message configuration
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);


    }

}
