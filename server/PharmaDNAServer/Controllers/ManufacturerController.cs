using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;
using PharmaDNAServer.Services;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/manufacturer")]
public class ManufacturerController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMilestoneService _milestoneService;

    public ManufacturerController(ApplicationDbContext context, IMilestoneService milestoneService)
    {
        _context = context;
        _milestoneService = milestoneService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNFTs([FromQuery] string? batchNumber, [FromQuery] string? name)
    {
        IQueryable<NFT> query = _context.NFTs;

        if (!string.IsNullOrEmpty(batchNumber))
        {
            query = query.Where(n => n.BatchNumber == batchNumber);
            var nft = await query.FirstOrDefaultAsync();
            if (nft == null) return Ok(new { });
            return Ok(nft);
        }

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(n => n.Name.Contains(name));
            var nft = await query.FirstOrDefaultAsync();
            if (nft == null) return Ok(new { });
            return Ok(nft);
        }

        var nfts = await query.ToListAsync();
        return Ok(nfts);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNFT([FromBody] CreateNFTRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Status) || string.IsNullOrEmpty(request.ManufacturerAddress))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var now = DateTime.UtcNow;
        var nft = new NFT
        {
            Name = request.Name,
            Status = request.Status,
            ManufacturerAddress = request.ManufacturerAddress,
            CreatedAt = now
        };

        _context.NFTs.Add(nft);
        await _context.SaveChangesAsync();

        return Ok(nft);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateNFT([FromBody] UpdateNFTRequest request)
    {
        if (request.Id == 0 || string.IsNullOrEmpty(request.Status))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var nft = await _context.NFTs.FindAsync(request.Id);
        if (nft == null)
        {
            return NotFound(new { error = "Không tìm thấy NFT" });
        }

        nft.Status = request.Status;
        _context.NFTs.Update(nft);
        await _context.SaveChangesAsync();

        return Ok(nft);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteNFT([FromBody] DeleteNFTRequest request)
    {
        if (request.Id == 0)
        {
            return BadRequest(new { error = "Thiếu id" });
        }

        var nft = await _context.NFTs.FindAsync(request.Id);
        if (nft != null)
        {
            _context.NFTs.Remove(nft);
            await _context.SaveChangesAsync();
        }

        return Ok(new { success = true });
    }

    // Transfer request endpoints
    [HttpGet("transfer-request")]
    public async Task<IActionResult> GetTransferRequests()
    {
        var requests = await _context.TransferRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return Ok(requests);
    }

    [HttpPost("transfer-request")]
    public async Task<IActionResult> CreateTransferRequest([FromBody] CreateTransferRequestRequest request)
    {
        if (request.NftId == 0 || string.IsNullOrEmpty(request.DistributorAddress))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var transferRequest = new TransferRequest
        {
            NftId = request.NftId,
            DistributorAddress = request.DistributorAddress,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.TransferRequests.Add(transferRequest);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, request = transferRequest });
    }

    [HttpPut("transfer-request")]
    public async Task<IActionResult> ApproveTransferRequest([FromBody] ApproveTransferRequestRequest request)
    {
        if (request.RequestId == 0 || request.NftId == 0 || string.IsNullOrEmpty(request.DistributorAddress))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var transferRequest = await _context.TransferRequests.FindAsync(request.RequestId);
        if (transferRequest == null)
        {
            return NotFound(new { error = "Không tìm thấy yêu cầu" });
        }

        transferRequest.Status = "approved";
        _context.TransferRequests.Update(transferRequest);

        var nft = await _context.NFTs.FindAsync(request.NftId);
        if (nft != null)
        {
            nft.DistributorAddress = request.DistributorAddress;
            nft.Status = "in_transit";
            _context.NFTs.Update(nft);
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // Milestone endpoints
    [HttpGet("milestone")]
    public async Task<IActionResult> GetMilestones([FromQuery] string? batchNumber, [FromQuery] int? nftId)
    {
        var milestones = await _milestoneService.GetMilestonesAsync(batchNumber, nftId);
        return Ok(milestones);
    }

    [HttpPost("milestone")]
    public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneRequest request)
    {
        try
        {
            var milestone = await _milestoneService.CreateMilestoneAsync(request);
            return Ok(new { success = true, milestone });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class CreateNFTRequest
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ManufacturerAddress { get; set; } = string.Empty;
}

public class UpdateNFTRequest
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DeleteNFTRequest
{
    public int Id { get; set; }
}

public class CreateTransferRequestRequest
{
    public int NftId { get; set; }
    public string DistributorAddress { get; set; } = string.Empty;
}

public class ApproveTransferRequestRequest
{
    public int RequestId { get; set; }
    public int NftId { get; set; }
    public string DistributorAddress { get; set; } = string.Empty;
}

public class CreateMilestoneRequest
{
    public int NftId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? Timestamp { get; set; }
    public string ActorAddress { get; set; } = string.Empty;
}

