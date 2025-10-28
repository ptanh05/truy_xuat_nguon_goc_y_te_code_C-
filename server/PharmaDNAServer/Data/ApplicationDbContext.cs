using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<NFT> NFTs { get; set; }
    public DbSet<TransferRequest> TransferRequests { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
}

