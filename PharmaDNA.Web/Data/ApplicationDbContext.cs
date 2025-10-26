using Microsoft.EntityFrameworkCore;
using PharmaDNA.Web.Models.Entities;

namespace PharmaDNA.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NFT> NFTs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TransferRequest> TransferRequests { get; set; }
        public DbSet<Milestone> Milestones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // NFT configuration
            modelBuilder.Entity<NFT>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ManufacturerAddress).IsRequired().HasMaxLength(42);
                entity.Property(e => e.DistributorAddress).HasMaxLength(42);
                entity.Property(e => e.PharmacyAddress).HasMaxLength(42);
                entity.Property(e => e.IpfsHash).HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);

                // Indexes
                entity.HasIndex(e => e.BatchNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ManufacturerAddress);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(42);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

                // Indexes
                entity.HasIndex(e => e.Address).IsUnique();
                entity.HasIndex(e => e.Role);
            });

            // TransferRequest configuration
            modelBuilder.Entity<TransferRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DistributorAddress).IsRequired().HasMaxLength(42);
                entity.Property(e => e.PharmacyAddress).IsRequired().HasMaxLength(42);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TransferNote).HasMaxLength(500);
                
                // Foreign key relationship
                entity.HasOne(e => e.NFT)
                    .WithMany(n => n.TransferRequests)
                    .HasForeignKey(e => e.NftId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.DistributorAddress);
                entity.HasIndex(e => e.PharmacyAddress);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.NftId);
            });

            // Milestone configuration
            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(255);
                entity.Property(e => e.ActorAddress).IsRequired().HasMaxLength(42);
                
                // Foreign key relationship
                entity.HasOne(e => e.NFT)
                    .WithMany(n => n.Milestones)
                    .HasForeignKey(e => e.NftId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.NftId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.ActorAddress);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Add any initial seed data here if needed
        }
    }
}
