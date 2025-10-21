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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // NFT configuration
            modelBuilder.Entity<NFT>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.ManufacturerAddress).HasMaxLength(42);
                entity.Property(e => e.DistributorAddress).HasMaxLength(42);
                entity.Property(e => e.PharmacyAddress).HasMaxLength(42);
                entity.HasIndex(e => e.BatchNumber).IsUnique();
                entity.HasMany(e => e.Milestones).WithOne(m => m.NFT).HasForeignKey(m => m.NFTId);
            });

            // TransferRequest configuration
            modelBuilder.Entity<TransferRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BatchNumber).HasMaxLength(100);
                entity.Property(e => e.DistributorAddress).IsRequired().HasMaxLength(42);
                entity.Property(e => e.PharmacyAddress).IsRequired().HasMaxLength(42);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.BlockchainTxHash).HasMaxLength(66);
                entity.Property(e => e.BlockchainStatus).HasMaxLength(20);
                entity.HasOne(e => e.NFT).WithMany().HasForeignKey(e => e.NFTId);
            });

            // Milestone configuration
            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(255);
                entity.Property(e => e.ActorAddress).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.NFT).WithMany(n => n.Milestones).HasForeignKey(e => e.NFTId);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecipientAddress).IsRequired().HasMaxLength(42);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
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
