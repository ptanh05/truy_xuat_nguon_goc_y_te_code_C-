using Microsoft.EntityFrameworkCore;
using PharmaDNA.Models;

namespace PharmaDNA.Data
{
    public class PharmaDNAContext : DbContext
    {
        public PharmaDNAContext(DbContextOptions<PharmaDNAContext> options)
            : base(options)
        {
        }

        public DbSet<NFT> NFTs { get; set; }
        public DbSet<TransferRequest> TransferRequests { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }
        
        // Core DbSets for Pharma Network
        public DbSet<TraceabilityRecord> TraceabilityRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // NFT configuration
            modelBuilder.Entity<NFT>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ProductCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BatchId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Manufacturer).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.ProductCode);
                entity.HasMany(e => e.TransferRequests).WithOne().HasForeignKey(t => t.NFTId);
            });

            // TransferRequest configuration
            modelBuilder.Entity<TransferRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FromAddress).HasMaxLength(100);
                entity.Property(e => e.ToAddress).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.BlockchainTransactionHash).HasMaxLength(100);
                entity.HasOne(e => e.NFT).WithMany().HasForeignKey(e => e.NFTId);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).IsRequired().HasMaxLength(42);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.WalletAddress).IsUnique();
            });
        }
    }
}
