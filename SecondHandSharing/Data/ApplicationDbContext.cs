using Microsoft.EntityFrameworkCore;
using SecondHandSharing.Models;

namespace SecondHandSharing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ViewHistory> ViewHistories { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precision cho giá tiền
            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

            // ============================
            // FIX MULTIPLE CASCADE PATHS
            // ============================

            // ViewHistory → UserAmount
            modelBuilder.Entity<ViewHistory>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ViewHistory → Item
            modelBuilder.Entity<ViewHistory>()
                .HasOne(v => v.Item)
                .WithMany()
                .HasForeignKey(v => v.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            // Favorite → User
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Favorite → Item
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Item)
                .WithMany()
                .HasForeignKey(f => f.ItemId)
                .OnDelete(DeleteBehavior.NoAction);
                // Conversation -> User1, User2
modelBuilder.Entity<Conversation>()
    .HasOne(c => c.User1)
    .WithMany()
    .HasForeignKey(c => c.User1Id)
    .OnDelete(DeleteBehavior.NoAction);

modelBuilder.Entity<Conversation>()
    .HasOne(c => c.User2)
    .WithMany()
    .HasForeignKey(c => c.User2Id)
    .OnDelete(DeleteBehavior.NoAction);

// Message -> Conversation
modelBuilder.Entity<Message>()
    .HasOne(m => m.Conversation)
    .WithMany(c => c.Messages)
    .HasForeignKey(m => m.ConversationId)
    .OnDelete(DeleteBehavior.NoAction);

// Message -> Sender (User)
modelBuilder.Entity<Message>()
    .HasOne(m => m.Sender)
    .WithMany()
    .HasForeignKey(m => m.SenderId)
    .OnDelete(DeleteBehavior.NoAction);

// Comment -> User, Item (tránh multiple cascade paths)
modelBuilder.Entity<Comment>()
        .HasOne(c => c.User)
        .WithMany()
        .HasForeignKey(c => c.UserId)
        .OnDelete(DeleteBehavior.NoAction);

    modelBuilder.Entity<Comment>()
        .HasOne(c => c.Item)
        .WithMany(i => i.Comments)
        .HasForeignKey(c => c.ItemId)
        .OnDelete(DeleteBehavior.NoAction);
    modelBuilder.Entity<Transaction>()
        .HasOne(t => t.Item)
        .WithMany()
        .HasForeignKey(t => t.ItemId)
        .OnDelete(DeleteBehavior.NoAction);

    // Transaction -> ServicePackage
    modelBuilder.Entity<Transaction>()
        .HasOne(t => t.Package)
        .WithMany(p => p.Transactions)
        .HasForeignKey(t => t.PackageId)
        .OnDelete(DeleteBehavior.NoAction);

    // (tuỳ chọn) khai báo khóa chính ServicePackage – thực ra [Key] là đủ
    modelBuilder.Entity<ServicePackage>()
        .HasKey(p => p.PackageId);
    // Precision tiền
        modelBuilder.Entity<Item>()
            .Property(i => i.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .Property(u => u.WalletBalance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);
            modelBuilder.Entity<Transaction>()
    .HasOne(t => t.Package)
    .WithMany()
    .HasForeignKey(t => t.PackageId)
    .OnDelete(DeleteBehavior.NoAction);   // quan hệ OPTIONAL

modelBuilder.Entity<Transaction>()
    .HasOne(t => t.Item)
    .WithMany()
    .HasForeignKey(t => t.ItemId)
    .OnDelete(DeleteBehavior.NoAction);

    // ✅ SEED GÓI VIP & BOOST
    modelBuilder.Entity<ServicePackage>().HasData(
        new ServicePackage { PackageId = 1, Name = "Boost 1 ngày", Type = "BOOST", Price = 20000,  DurationDays = 1 },
        new ServicePackage { PackageId = 2, Name = "Boost 3 ngày", Type = "BOOST", Price = 50000,  DurationDays = 3 },
        new ServicePackage { PackageId = 3, Name = "Boost 7 ngày", Type = "BOOST", Price = 100000, DurationDays = 7 },
        new ServicePackage { PackageId = 4, Name = "VIP 7 ngày",   Type = "VIP",   Price = 150000, DurationDays = 7 },
        new ServicePackage { PackageId = 5, Name = "VIP 30 ngày",  Type = "VIP",   Price = 400000, DurationDays = 30 }
    );
        }
    }
}
