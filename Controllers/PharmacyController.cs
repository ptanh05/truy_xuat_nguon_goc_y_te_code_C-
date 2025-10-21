using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Data;
using PharmaDNA.Models;
using PharmaDNA.Services;
using PharmaDNA.Attributes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireRole("Pharmacy")]
    public class PharmacyController : ControllerBase
    {
        private readonly PharmaDNAContext _context;
        private readonly INFTService _nftService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PharmacyController> _logger;

        public PharmacyController(
            PharmaDNAContext context,
            INFTService nftService,
            INotificationService notificationService,
            ILogger<PharmacyController> logger)
        {
            _context = context;
            _nftService = nftService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNFTs([FromQuery] string pharmacyAddress)
        {
            try
            {
                var nfts = await _nftService.GetNFTsByPharmacyAsync(pharmacyAddress);
                return Ok(new { success = true, data = nfts });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFTs: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("batch/{batchNumber}")]
        public async Task<IActionResult> GetNFTByBatchNumber(string batchNumber)
        {
            try
            {
                var nft = await _nftService.GetNFTByBatchNumberAsync(batchNumber);
                if (nft == null)
                    return NotFound(new { success = false, message = "NFT not found" });

                return Ok(new { success = true, data = nft });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFT: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("transfer-requests")]
        public async Task<IActionResult> GetTransferRequests([FromQuery] string pharmacyAddress)
        {
            try
            {
                var requests = await _context.TransferRequests
                    .Where(r => r.PharmacyAddress == pharmacyAddress && r.Status == "pending")
                    .Include(r => r.NFT)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transfer requests: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("transfer-requests/{id}/approve")]
        public async Task<IActionResult> ApproveTransferRequest(int id, [FromBody] ApproveTransferDto request)
        {
            try
            {
                var transferRequest = await _context.TransferRequests.Include(r => r.NFT).FirstOrDefaultAsync(r => r.Id == id);
                if (transferRequest == null)
                    return NotFound(new { success = false, message = "Transfer request not found" });

                // Check expiration
                if (DateTime.UtcNow > transferRequest.ExpiresAt)
                    return BadRequest(new { success = false, message = "Transfer request expired" });

                transferRequest.Status = "approved";
                transferRequest.BlockchainStatus = "confirmed";
                transferRequest.UpdatedAt = DateTime.UtcNow;

                // Update NFT
                transferRequest.NFT.PharmacyAddress = transferRequest.PharmacyAddress;
                transferRequest.NFT.DistributorAddress = null;
                transferRequest.NFT.Status = "transferred_to_pharmacy";
                transferRequest.NFT.UpdatedAt = DateTime.UtcNow;

                // Create warehouse confirmation milestone
                var milestone = new Milestone
                {
                    NFTId = transferRequest.NFTId,
                    Type = "Warehouse Confirmation",
                    Description = "Nhà thuốc xác nhận đã nhận lô thuốc",
                    Location = request.WarehouseLocation,
                    Timestamp = DateTime.UtcNow,
                    ActorAddress = transferRequest.PharmacyAddress
                };

                _context.TransferRequests.Update(transferRequest);
                _context.Milestones.Add(milestone);
                await _context.SaveChangesAsync();

                // Create notification for distributor
                var notification = new Notification
                {
                    RecipientAddress = transferRequest.DistributorAddress,
                    Type = "transfer_approved",
                    Title = "Yêu cầu chuyển lô được duyệt",
                    Message = $"Nhà thuốc đã duyệt yêu cầu chuyển lô {transferRequest.NFT.BatchNumber}",
                    Data = JsonConvert.SerializeObject(new { requestId = id, nftId = transferRequest.NFTId }),
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.CreateNotificationAsync(notification);

                return Ok(new { success = true, data = transferRequest });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving transfer request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("transfer-requests/{id}/reject")]
        public async Task<IActionResult> RejectTransferRequest(int id, [FromBody] RejectTransferDto request)
        {
            try
            {
                var transferRequest = await _context.TransferRequests.Include(r => r.NFT).FirstOrDefaultAsync(r => r.Id == id);
                if (transferRequest == null)
                    return NotFound(new { success = false, message = "Transfer request not found" });

                transferRequest.Status = "rejected";
                transferRequest.BlockchainStatus = "failed";
                transferRequest.UpdatedAt = DateTime.UtcNow;

                _context.TransferRequests.Update(transferRequest);
                await _context.SaveChangesAsync();

                // Create notification for distributor
                var notification = new Notification
                {
                    RecipientAddress = transferRequest.DistributorAddress,
                    Type = "transfer_rejected",
                    Title = "Yêu cầu chuyển lô bị từ chối",
                    Message = $"Nhà thuốc đã từ chối yêu cầu chuyển lô {transferRequest.NFT.BatchNumber}. Lý do: {request.Reason}",
                    Data = JsonConvert.SerializeObject(new { requestId = id, nftId = transferRequest.NFTId }),
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.CreateNotificationAsync(notification);

                return Ok(new { success = true, data = transferRequest });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting transfer request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetNFTHistory(int id)
        {
            try
            {
                var nft = await _nftService.GetNFTByIdAsync(id);
                if (nft == null)
                    return NotFound(new { success = false, message = "NFT not found" });

                return Ok(new { success = true, data = nft.Milestones });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFT history: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class ApproveTransferDto
    {
        public string WarehouseLocation { get; set; }
    }

    public class RejectTransferDto
    {
        public string Reason { get; set; }
    }
}
