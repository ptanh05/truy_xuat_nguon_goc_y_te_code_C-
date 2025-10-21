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
    [RequireRole("Distributor")]
    public class DistributorController : ControllerBase
    {
        private readonly PharmaDNAContext _context;
        private readonly INFTService _nftService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DistributorController> _logger;

        public DistributorController(
            PharmaDNAContext context,
            INFTService nftService,
            INotificationService notificationService,
            ILogger<DistributorController> logger)
        {
            _context = context;
            _nftService = nftService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNFTs([FromQuery] string distributorAddress)
        {
            try
            {
                var nfts = await _nftService.GetNFTsByDistributorAsync(distributorAddress);
                return Ok(new { success = true, data = nfts });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFTs: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("transfer-requests")]
        public async Task<IActionResult> GetTransferRequests([FromQuery] string distributorAddress)
        {
            try
            {
                var requests = await _context.TransferRequests
                    .Where(r => r.DistributorAddress == distributorAddress)
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

        [HttpPost("transfer-to-pharmacy")]
        public async Task<IActionResult> CreateTransferRequest([FromBody] CreateTransferRequestDto request)
        {
            try
            {
                var nft = await _nftService.GetNFTByIdAsync(request.NFTId);
                if (nft == null)
                    return NotFound(new { success = false, message = "NFT not found" });

                // Verify distributor ownership
                if (nft.DistributorAddress != request.DistributorAddress)
                    return BadRequest(new { success = false, message = "Not authorized" });

                var transferRequest = new TransferRequest
                {
                    NFTId = request.NFTId,
                    BatchNumber = nft.BatchNumber,
                    DistributorAddress = request.DistributorAddress,
                    PharmacyAddress = request.PharmacyAddress,
                    Status = "pending",
                    BlockchainStatus = "pending",
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                };

                _context.TransferRequests.Add(transferRequest);
                await _context.SaveChangesAsync();

                // Create notification for pharmacy
                var notification = new Notification
                {
                    RecipientAddress = request.PharmacyAddress,
                    Type = "transfer_request",
                    Title = "Yêu cầu chuyển lô thuốc",
                    Message = $"Nhà phân phối muốn chuyển lô {nft.BatchNumber}",
                    Data = JsonConvert.SerializeObject(new { requestId = transferRequest.Id, nftId = nft.Id }),
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.CreateNotificationAsync(notification);

                return Ok(new { success = true, data = transferRequest });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating transfer request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("transfer-requests/{id}")]
        public async Task<IActionResult> UpdateTransferRequest(int id, [FromBody] UpdateTransferRequestDto request)
        {
            try
            {
                var transferRequest = await _context.TransferRequests.Include(r => r.NFT).FirstOrDefaultAsync(r => r.Id == id);
                if (transferRequest == null)
                    return NotFound(new { success = false, message = "Transfer request not found" });

                transferRequest.Status = request.Status;
                transferRequest.BlockchainStatus = request.Status == "approved" ? "confirmed" : "failed";
                transferRequest.UpdatedAt = DateTime.UtcNow;

                if (request.Status == "approved")
                {
                    // Update NFT ownership
                    transferRequest.NFT.PharmacyAddress = transferRequest.PharmacyAddress;
                    transferRequest.NFT.DistributorAddress = null;
                    transferRequest.NFT.Status = "transferred_to_pharmacy";
                    transferRequest.NFT.UpdatedAt = DateTime.UtcNow;

                    // Create milestone
                    var milestone = new Milestone
                    {
                        NFTId = transferRequest.NFTId,
                        Type = "Pharmacy Transfer",
                        Description = "Lô thuốc được chuyển đến nhà thuốc",
                        Location = "Nhà thuốc",
                        Timestamp = DateTime.UtcNow,
                        ActorAddress = transferRequest.PharmacyAddress
                    };
                    _context.Milestones.Add(milestone);
                }

                _context.TransferRequests.Update(transferRequest);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = transferRequest });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating transfer request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("transfer-requests/{id}")]
        public async Task<IActionResult> CancelTransferRequest(int id)
        {
            try
            {
                var transferRequest = await _context.TransferRequests.FindAsync(id);
                if (transferRequest == null)
                    return NotFound(new { success = false, message = "Transfer request not found" });

                _context.TransferRequests.Remove(transferRequest);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Transfer request cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling transfer request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class CreateTransferRequestDto
    {
        public int NFTId { get; set; }
        public string DistributorAddress { get; set; }
        public string PharmacyAddress { get; set; }
    }

    public class UpdateTransferRequestDto
    {
        public string Status { get; set; }
    }
}
