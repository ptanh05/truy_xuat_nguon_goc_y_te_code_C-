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
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Tên thuốc không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { error = "Trạng thái không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.ManufacturerAddress))
        {
            return BadRequest(new { error = "Địa chỉ nhà sản xuất không được để trống" });
        }

        // Kiểm tra batch number đã tồn tại chưa (nếu có)
        if (!string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            var existingNft = await _context.NFTs
                .FirstOrDefaultAsync(n => n.BatchNumber == request.BatchNumber);
            if (existingNft != null)
            {
                return BadRequest(new { error = $"Số lô {request.BatchNumber} đã tồn tại trong hệ thống" });
            }
        }

        var now = DateTime.UtcNow;
        var nft = new NFT
        {
            Name = request.Name,
            BatchNumber = request.BatchNumber,
            Gtin = request.Gtin,
            ManufactureDate = request.ManufactureDate,
            ExpiryDate = request.ExpiryDate,
            Description = request.Description,
            Formulation = request.Formulation,
            ImageUrl = request.ImageUrl,
            CertificateUrl = request.CertificateUrl,
            IpfsHash = request.IpfsHash,
            Status = request.Status,
            ManufacturerAddress = request.ManufacturerAddress.ToLower(),
            CreatedAt = now
        };

        try
        {
            _context.NFTs.Add(nft);
            await _context.SaveChangesAsync();

            return Ok(nft);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Lỗi khi lưu NFT vào database", message = ex.Message });
        }
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

        // Validate address format
        if (!request.DistributorAddress.StartsWith("0x") || request.DistributorAddress.Length != 42)
        {
            return BadRequest(new { error = "Địa chỉ nhà phân phối không hợp lệ" });
        }

        // Sử dụng transaction để đảm bảo data consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var transferRequest = await _context.TransferRequests.FindAsync(request.RequestId);
            if (transferRequest == null)
            {
                return NotFound(new { error = "Không tìm thấy yêu cầu" });
            }

            if (transferRequest.Status != "pending")
            {
                return BadRequest(new { error = $"Yêu cầu đã được xử lý. Trạng thái hiện tại: {transferRequest.Status}" });
            }

            var nft = await _context.NFTs.FindAsync(request.NftId);
            if (nft == null)
            {
                return NotFound(new { error = "Không tìm thấy NFT" });
            }

            // Update transfer request
            transferRequest.Status = "approved";
            transferRequest.UpdatedAt = DateTime.UtcNow;
            _context.TransferRequests.Update(transferRequest);

            // Update NFT
            nft.DistributorAddress = request.DistributorAddress.ToLower();
            nft.Status = "in_transit";
            _context.NFTs.Update(nft);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = "Lỗi khi duyệt yêu cầu chuyển giao", message = ex.Message });
        }
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
    public string? BatchNumber { get; set; }
    public string? Gtin { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
    public string? Formulation { get; set; }
    public string? ImageUrl { get; set; }
    public string? CertificateUrl { get; set; }
    public string? IpfsHash { get; set; }
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

