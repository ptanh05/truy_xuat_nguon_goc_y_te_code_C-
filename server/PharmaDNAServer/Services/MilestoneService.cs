using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;
using PharmaDNAServer.Controllers;

namespace PharmaDNAServer.Services;

public interface IMilestoneService
{
    Task<IEnumerable<Milestone>> GetMilestonesAsync(string? batchNumber, int? nftId);
    Task<Milestone> CreateMilestoneAsync(CreateMilestoneRequest request);
}

public class MilestoneService : IMilestoneService
{
    private readonly ApplicationDbContext _context;

    public MilestoneService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Milestone>> GetMilestonesAsync(string? batchNumber, int? nftId)
    {
        IQueryable<Milestone> query = _context.Milestones;

        if (!string.IsNullOrEmpty(batchNumber))
        {
            var nft = await _context.NFTs.FirstOrDefaultAsync(n => n.BatchNumber == batchNumber);
            if (nft == null) return Enumerable.Empty<Milestone>();
            query = query.Where(m => m.NftId == nft.Id);
        }
        else if (nftId.HasValue)
        {
            query = query.Where(m => m.NftId == nftId.Value);
        }
        else
        {
            return Enumerable.Empty<Milestone>();
        }

        return await query.OrderBy(m => m.Timestamp).ToListAsync();
    }

    public async Task<Milestone> CreateMilestoneAsync(CreateMilestoneRequest request)
    {
        if (request.NftId == 0 || string.IsNullOrEmpty(request.Type) || string.IsNullOrEmpty(request.ActorAddress))
        {
            throw new ArgumentException("Thiếu thông tin bắt buộc");
        }

        var milestone = new Milestone
        {
            NftId = request.NftId,
            Type = request.Type,
            Description = request.Description,
            Location = request.Location,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            ActorAddress = request.ActorAddress
        };

        _context.Milestones.Add(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }
}


