using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } // maps to NguoiDung
    public DbSet<NFT> NFTs { get; set; } // maps to SanPhamNFT
    public DbSet<TransferRequest> TransferRequests { get; set; } // maps to YeuCauChuyen
    public DbSet<Milestone> Milestones { get; set; } // maps to MocDanhDau
    public DbSet<SensorLog> SensorLogs { get; set; } // maps to SensorLogs
}

