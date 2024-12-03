using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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


    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Payout> Payouts { get; set; }
    public DbSet<Subscription> Subscriptions { get; internal set; }

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


        // Transactions configuration
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Sender)
            .WithMany()
            .HasForeignKey(t => t.SenderId);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Recipient)
            .WithMany()
            .HasForeignKey(t => t.RecipientId);

        modelBuilder.Entity<Wallet>()
            .HasOne(w => w.User)
            .WithOne()
            .HasForeignKey<Wallet>(w => w.UserId);

        modelBuilder.Entity<Payout>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

    }


    public override int SaveChanges()
    {
        HandleAuditableEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private int? GetUserId()
    {
        var httpContextAccessor = this.GetService<IHttpContextAccessor>();
        var userIdClaim = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    private void HandleAuditableEntities()
    {
        var userId = GetUserId();

        // Handle entities implementing IOwnable
        foreach (var entry in ChangeTracker.Entries<IOwnable>())
        {
            if (entry.State == EntityState.Added && userId != null)
            {
                entry.Entity.UserId = userId.Value;
            }
        }

        
        // Handle entities implementing ICreatable
        foreach (var entry in ChangeTracker.Entries<ICreatable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }

        // Handle entities implementing IUpdatable
        foreach (var entry in ChangeTracker.Entries<IUpdatable>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Handle entities implementing IDeletable
        foreach (var entry in ChangeTracker.Entries<IDeletable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Active = true;
                entry.Entity.DeletedAt = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (!entry.Entity.Active && entry.Entity.DeletedAt == null)
                {
                    // Set DeletedAt when deactivating
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                }
                else if (entry.Entity.Active && entry.Entity.DeletedAt != null)
                {
                    // Clear DeletedAt when reactivating
                    entry.Entity.DeletedAt = null;
                }
            }
        }
    }
}
