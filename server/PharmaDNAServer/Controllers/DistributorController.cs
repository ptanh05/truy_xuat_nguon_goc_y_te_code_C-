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
        if (request.NftId == 0)
        {
            return BadRequest(new { error = "NFT ID không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.PharmacyAddress))
        {
            return BadRequest(new { error = "Địa chỉ nhà thuốc không được để trống" });
        }

        // Validate address format (basic check)
        if (!request.PharmacyAddress.StartsWith("0x") || request.PharmacyAddress.Length != 42)
        {
            return BadRequest(new { error = "Địa chỉ nhà thuốc không hợp lệ" });
        }

        var distributorAddress = Request.Headers["x-distributor-address"].ToString();
        if (string.IsNullOrEmpty(distributorAddress))
        {
            return BadRequest(new { error = "Thiếu địa chỉ nhà phân phối" });
        }

        // Validate distributor address format
        if (!distributorAddress.StartsWith("0x") || distributorAddress.Length != 42)
        {
            return BadRequest(new { error = "Địa chỉ nhà phân phối không hợp lệ" });
        }

        // Kiểm tra NFT có tồn tại và đang ở trạng thái phù hợp không
        var nft = await _context.NFTs.FindAsync(request.NftId);
        if (nft == null)
        {
            return NotFound(new { error = $"Không tìm thấy NFT với ID {request.NftId}" });
        }

        if (nft.Status != "received" && nft.Status != "in_transit")
        {
            return BadRequest(new { error = $"NFT không ở trạng thái phù hợp để chuyển. Trạng thái hiện tại: {nft.Status}" });
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
            return BadRequest(new { error = "Thiếu thông tin bắt buộc" });
        }

        if (request.Status != "approved" && request.Status != "rejected")
        {
            return BadRequest(new { error = "Trạng thái không hợp lệ. Chỉ chấp nhận: approved, rejected" });
        }

        // Validate address format
        if (!request.PharmacyAddress.StartsWith("0x") || request.PharmacyAddress.Length != 42)
        {
            return BadRequest(new { error = "Địa chỉ nhà thuốc không hợp lệ" });
        }

        // Sử dụng transaction để đảm bảo data consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var transferRequest = await _context.TransferRequests.FindAsync(request.RequestId);
            if (transferRequest == null)
            {
                return NotFound(new { error = "Không tìm thấy yêu cầu chuyển giao" });
            }

            if (transferRequest.PharmacyAddress != request.PharmacyAddress.ToLower())
            {
                return NotFound(new { error = "Không tìm thấy yêu cầu chuyển giao" });
            }

            if (transferRequest.Status != "pending")
            {
                return BadRequest(new { error = $"Yêu cầu đã được xử lý. Trạng thái hiện tại: {transferRequest.Status}" });
            }

            transferRequest.Status = request.Status;
            transferRequest.UpdatedAt = DateTime.UtcNow;

            if (request.Status == "approved")
            {
                var nft = await _context.NFTs.FindAsync(transferRequest.NftId);
                if (nft == null)
                {
                    return NotFound(new { error = "Không tìm thấy NFT" });
                }

                // Kiểm tra NFT có ở trạng thái phù hợp không
                if (nft.Status != "received" && nft.Status != "in_transit")
                {
                    return BadRequest(new { error = $"NFT không ở trạng thái phù hợp để chuyển. Trạng thái hiện tại: {nft.Status}" });
                }

                nft.PharmacyAddress = request.PharmacyAddress.ToLower();
                nft.Status = "in_pharmacy";
                _context.NFTs.Update(nft);
            }

            _context.TransferRequests.Update(transferRequest);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = "Lỗi khi cập nhật yêu cầu chuyển giao", message = ex.Message });
        }
    }

    [HttpDelete("transfer-to-pharmacy")]
    public async Task<IActionResult> CancelTransferRequest([FromBody] CancelTransferRequestRequest request)
    {
        if (request.RequestId == 0)
        {
            return BadRequest(new { error = "Thiếu Request ID" });
        }

        var distributorAddress = Request.Headers["x-distributor-address"].ToString();
        if (string.IsNullOrEmpty(distributorAddress))
        {
            return BadRequest(new { error = "Thiếu địa chỉ nhà phân phối" });
        }

        var transferRequest = await _context.TransferRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId 
                && r.DistributorAddress == distributorAddress.ToLower() 
                && r.Status == "pending");

        if (transferRequest == null)
        {
            return NotFound(new { error = "Không tìm thấy yêu cầu chuyển giao hoặc yêu cầu không thể hủy" });
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

    /// <summary>
    /// Xác nhận nhận hàng từ manufacturer
    /// Cập nhật status NFT và tạo milestone "Đã nhận hàng"
    /// </summary>
    [HttpPost("confirm-receipt")]
    public async Task<IActionResult> ConfirmReceipt([FromBody] ConfirmReceiptRequest request)
    {
        if (request.NftId == 0)
        {
            return BadRequest(new { error = "Thiếu NFT ID" });
        }

        var distributorAddress = Request.Headers["x-distributor-address"].ToString();
        if (string.IsNullOrEmpty(distributorAddress))
        {
            distributorAddress = request.DistributorAddress;
        }

        if (string.IsNullOrEmpty(distributorAddress))
        {
            return BadRequest(new { error = "Thiếu địa chỉ nhà phân phối" });
        }

        // Sử dụng transaction để đảm bảo data consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Tìm NFT
            var nft = await _context.NFTs.FindAsync(request.NftId);
            if (nft == null)
            {
                return NotFound(new { error = "Không tìm thấy NFT" });
            }

            // Kiểm tra transfer request đã được approved chưa
            var transferRequest = await _context.TransferRequests
                .FirstOrDefaultAsync(r => r.NftId == request.NftId 
                    && r.DistributorAddress == distributorAddress.ToLower() 
                    && r.Status == "approved");

            if (transferRequest == null)
            {
                return BadRequest(new { error = "Chưa có yêu cầu chuyển giao được duyệt cho NFT này" });
            }

            // Cập nhật status NFT
            nft.Status = "received";
            nft.DistributorAddress = distributorAddress.ToLower();
            _context.NFTs.Update(nft);

            // Tạo milestone "Đã nhận hàng"
            var milestone = new Milestone
            {
                NftId = request.NftId,
                Type = "Đã nhận hàng",
                Description = request.Note ?? "Nhà phân phối xác nhận đã nhận lô thuốc từ nhà sản xuất",
                Location = request.Location,
                Timestamp = DateTime.UtcNow,
                ActorAddress = distributorAddress.ToLower()
            };
            _context.Milestones.Add(milestone);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                success = true,
                message = $"✅ Đã xác nhận nhận hàng NFT #{request.NftId} thành công!",
                nft = new
                {
                    id = nft.Id,
                    status = nft.Status,
                    distributorAddress = nft.DistributorAddress
                },
                milestone = new
                {
                    id = milestone.Id,
                    type = milestone.Type,
                    timestamp = milestone.Timestamp
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Confirm receipt failed for NFT #{NftId}", request.NftId);
            return StatusCode(500, new { error = "Lỗi khi xác nhận nhận hàng", message = ex.Message });
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

public class ConfirmReceiptRequest
{
    public int NftId { get; set; }
    public string? DistributorAddress { get; set; }
    public string? Note { get; set; }
    public string? Location { get; set; }
}

