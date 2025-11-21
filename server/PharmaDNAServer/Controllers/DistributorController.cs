using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;
using PharmaDNAServer.Services;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/distributor")]
public class DistributorController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISensorService _sensorService;
    private readonly ILogger<DistributorController> _logger;

    public DistributorController(ApplicationDbContext context, ISensorService sensorService, ILogger<DistributorController> logger)
    {
        _context = context;
        _sensorService = sensorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetNFTsInTransit()
    {
        var nfts = await _context.NFTs
            .Where(n => n.Status == "in_transit")
            .ToListAsync();
        return Ok(nfts);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetDistributors()
    {
        var distributors = await _context.Users
            .Where(u => u.Role == "DISTRIBUTOR")
            .Select(u => new { address = u.Address })
            .ToListAsync();
        return Ok(distributors);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateNFT([FromBody] DistributorUpdateNFTRequest request)
    {
        if (request.Id == 0 || string.IsNullOrEmpty(request.Status) || string.IsNullOrEmpty(request.DistributorAddress))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var nft = await _context.NFTs.FindAsync(request.Id);
        if (nft == null)
        {
            return NotFound(new { error = "Không tìm thấy NFT" });
        }

        nft.Status = request.Status;
        nft.DistributorAddress = request.DistributorAddress;
        _context.NFTs.Update(nft);
        await _context.SaveChangesAsync();

        return Ok(nft);
    }

    // Transfer to pharmacy endpoints
    [HttpGet("transfer-to-pharmacy")]
    public async Task<IActionResult> GetTransferRequests(
        [FromQuery] string? distributorAddress,
        [FromQuery] string? pharmacyAddress,
        [FromQuery] string? status)
    {
        IQueryable<TransferRequest> query = _context.TransferRequests;

        if (!string.IsNullOrEmpty(distributorAddress))
        {
            query = query.Where(r => r.DistributorAddress == distributorAddress.ToLower());
        }

        if (!string.IsNullOrEmpty(pharmacyAddress))
        {
            query = query.Where(r => r.PharmacyAddress == pharmacyAddress.ToLower());
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost("transfer-to-pharmacy")]
    public async Task<IActionResult> CreateTransferToPharmacy([FromBody] CreateTransferToPharmacyRequest request)
    {
        if (request.NftId == 0 || string.IsNullOrEmpty(request.PharmacyAddress))
        {
            return BadRequest(new { error = "Missing required fields" });
        }

        var distributorAddress = Request.Headers["x-distributor-address"].ToString();
        if (string.IsNullOrEmpty(distributorAddress))
        {
            return BadRequest(new { error = "Distributor address required" });
        }

        var transferRequest = new TransferRequest
        {
            NftId = request.NftId,
            DistributorAddress = distributorAddress.ToLower(),
            PharmacyAddress = request.PharmacyAddress.ToLower(),
            TransferNote = request.TransferNote,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.TransferRequests.Add(transferRequest);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Id = transferRequest.Id,
            NftId = transferRequest.NftId,
            DistributorAddress = transferRequest.DistributorAddress,
            PharmacyAddress = transferRequest.PharmacyAddress,
            TransferNote = transferRequest.TransferNote,
            Status = transferRequest.Status,
            CreatedAt = transferRequest.CreatedAt,
            message = $"Yêu cầu chuyển lô NFT #{transferRequest.NftId} đã được tạo thành công! Đang chờ nhà thuốc xác nhận."
        });
    }

    [HttpPut("transfer-to-pharmacy")]
    public async Task<IActionResult> UpdateTransferRequest([FromBody] UpdateTransferRequestRequest request)
    {
        if (request.RequestId == 0 || string.IsNullOrEmpty(request.Status) || string.IsNullOrEmpty(request.PharmacyAddress))
        {
            return BadRequest(new { error = "Missing required fields" });
        }

        if (request.Status != "approved" && request.Status != "rejected")
        {
            return BadRequest(new { error = "Invalid status" });
        }

        var transferRequest = await _context.TransferRequests.FindAsync(request.RequestId);
        if (transferRequest == null)
        {
            return NotFound(new { error = "Transfer request not found" });
        }

        if (transferRequest.PharmacyAddress != request.PharmacyAddress.ToLower())
        {
            return NotFound(new { error = "Transfer request not found" });
        }

        transferRequest.Status = request.Status;
        transferRequest.UpdatedAt = DateTime.UtcNow;

        if (request.Status == "approved")
        {
            var nft = await _context.NFTs.FindAsync(transferRequest.NftId);
            if (nft != null)
            {
                nft.PharmacyAddress = request.PharmacyAddress;
                nft.Status = "in_pharmacy";
                _context.NFTs.Update(nft);
            }
        }

        _context.TransferRequests.Update(transferRequest);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Id = transferRequest.Id,
            NftId = transferRequest.NftId,
            DistributorAddress = transferRequest.DistributorAddress,
            PharmacyAddress = transferRequest.PharmacyAddress,
            TransferNote = transferRequest.TransferNote,
            Status = transferRequest.Status,
            CreatedAt = transferRequest.CreatedAt,
            UpdatedAt = transferRequest.UpdatedAt,
            message = transferRequest.Status == "approved"
                ? $"✅ Đã duyệt yêu cầu chuyển lô NFT #{transferRequest.NftId} thành công! NFT đã được chuyển quyền sở hữu."
                : $"❌ Đã từ chối yêu cầu chuyển lô NFT #{transferRequest.NftId}."
        });
    }

    [HttpDelete("transfer-to-pharmacy")]
    public async Task<IActionResult> CancelTransferRequest([FromBody] CancelTransferRequestRequest request)
    {
        if (request.RequestId == 0)
        {
            return BadRequest(new { error = "Missing required fields" });
        }

        var distributorAddress = Request.Headers["x-distributor-address"].ToString();
        if (string.IsNullOrEmpty(distributorAddress))
        {
            return BadRequest(new { error = "Missing required fields" });
        }

        var transferRequest = await _context.TransferRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId 
                && r.DistributorAddress == distributorAddress.ToLower() 
                && r.Status == "pending");

        if (transferRequest == null)
        {
            return NotFound(new { error = "Transfer request not found or cannot be cancelled" });
        }

        _context.TransferRequests.Remove(transferRequest);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = $"✅ Đã hủy yêu cầu chuyển lô NFT #{transferRequest.NftId} thành công!"
        });
    }

    [HttpPost("upload-sensor")]
    public async Task<IActionResult> UploadSensorData([FromForm] SensorUploadForm form)
    {
        if (form.SensorData == null || form.SensorData.Length == 0)
        {
            return BadRequest(new { error = "Thiếu file dữ liệu cảm biến" });
        }

        try
        {
            await using var memoryStream = new MemoryStream();
            await form.SensorData.CopyToAsync(memoryStream);

            var context = new SensorUploadContext(
                form.NftId,
                form.DistributorAddress ?? string.Empty,
                memoryStream.ToArray(),
                form.SensorData.ContentType
            );

            var result = await _sensorService.SaveSensorDataAsync(context);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sensor upload failed for NFT #{NftId}", form.NftId);
            return StatusCode(500, new { error = "Lỗi xử lý dữ liệu cảm biến" });
        }
    }
}

public class DistributorUpdateNFTRequest
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DistributorAddress { get; set; } = string.Empty;
}

public class CreateTransferToPharmacyRequest
{
    public int NftId { get; set; }
    public string PharmacyAddress { get; set; } = string.Empty;
    public string? TransferNote { get; set; }
}

public class UpdateTransferRequestRequest
{
    public int RequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PharmacyAddress { get; set; } = string.Empty;
}

public class CancelTransferRequestRequest
{
    public int RequestId { get; set; }
}

public class SensorUploadForm
{
    public int NftId { get; set; }
    public string? DistributorAddress { get; set; }
    public IFormFile? SensorData { get; set; }
}

