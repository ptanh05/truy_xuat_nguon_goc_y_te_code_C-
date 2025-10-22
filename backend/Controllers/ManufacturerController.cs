using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;
using PharmaDNA.Attributes;
using Newtonsoft.Json;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireRole("Manufacturer")]
    public class ManufacturerController : ControllerBase
    {
        private readonly INFTService _nftService;
        private readonly IBlockchainService _blockchainService;
        private readonly IPinataService _pinataService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ManufacturerController> _logger;

        public ManufacturerController(
            INFTService nftService,
            IBlockchainService blockchainService,
            IPinataService pinataService,
            INotificationService notificationService,
            ILogger<ManufacturerController> logger)
        {
            _nftService = nftService;
            _blockchainService = blockchainService;
            _pinataService = pinataService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNFTs([FromQuery] string manufacturerAddress)
        {
            try
            {
                var nfts = await _nftService.GetNFTsByManufacturerAsync(manufacturerAddress);
                return Ok(new { success = true, data = nfts });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFTs: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNFT([FromForm] CreateNFTRequest request)
        {
            try
            {
                // Upload metadata to Pinata
                var metadata = new Dictionary<string, object>
                {
                    { "drugName", request.DrugName },
                    { "batchNumber", request.BatchNumber },
                    { "manufacturer", request.ManufacturerAddress },
                    { "expiryDate", request.ExpiryDate },
                    { "quantity", request.Quantity },
                    { "description", request.Description }
                };

                var metadataHash = await _pinataService.UploadMetadataAsync(metadata);

                // Create NFT in database
                var nft = new NFT
                {
                    Name = request.DrugName,
                    BatchNumber = request.BatchNumber,
                    ManufacturerAddress = request.ManufacturerAddress,
                    Status = "pending",
                    MetadataUrl = $"ipfs://{metadataHash}",
                    CreatedAt = DateTime.UtcNow
                };

                // Upload image if provided
                if (request.DrugImage != null)
                {
                    var imageMetadata = new Dictionary<string, string>
                    {
                        { "name", $"{request.DrugName}-{request.BatchNumber}" }
                    };
                    var imageHash = await _pinataService.UploadFileAsync(request.DrugImage, imageMetadata);
                    nft.ImageUrl = $"ipfs://{imageHash}";
                }

                // Mint NFT on blockchain
                var txHash = await _blockchainService.MintNFTAsync(nft, nft.MetadataUrl);
                nft.Status = "active";

                // Save to database
                var createdNFT = await _nftService.CreateNFTAsync(nft);

                // Create milestone
                var milestone = new Milestone
                {
                    NFTId = createdNFT.Id,
                    Type = "Manufacturing",
                    Description = "Lô thuốc được sản xuất",
                    Location = request.ManufacturerLocation,
                    Timestamp = DateTime.UtcNow,
                    ActorAddress = request.ManufacturerAddress
                };

                return Ok(new { success = true, data = createdNFT, txHash });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating NFT: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNFT(int id, [FromBody] UpdateNFTRequest request)
        {
            try
            {
                var nft = await _nftService.GetNFTByIdAsync(id);
                if (nft == null)
                    return NotFound(new { success = false, message = "NFT not found" });

                nft.Status = request.Status;
                nft.UpdatedAt = DateTime.UtcNow;

                var updatedNFT = await _nftService.UpdateNFTAsync(nft);
                return Ok(new { success = true, data = updatedNFT });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating NFT: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> TransferToDistributor(int id, [FromBody] TransferRequest transferRequest)
        {
            try
            {
                var nft = await _nftService.GetNFTByIdAsync(id);
                if (nft == null)
                    return NotFound(new { success = false, message = "NFT not found" });

                // Verify ownership
                if (nft.ManufacturerAddress != transferRequest.DistributorAddress)
                {
                    // Transfer on blockchain
                    var txHash = await _blockchainService.TransferNFTAsync(id, transferRequest.DistributorAddress);

                    // Update NFT
                    nft.DistributorAddress = transferRequest.DistributorAddress;
                    nft.Status = "transferred_to_distributor";
                    await _nftService.UpdateNFTAsync(nft);

                    // Create notification for distributor
                    var notification = new Notification
                    {
                        RecipientAddress = transferRequest.DistributorAddress,
                        Type = "transfer_received",
                        Title = "Nhận lô thuốc mới",
                        Message = $"Bạn đã nhận lô thuốc {nft.BatchNumber} từ nhà sản xuất",
                        Data = JsonConvert.SerializeObject(new { nftId = nft.Id, batchNumber = nft.BatchNumber }),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationService.CreateNotificationAsync(notification);

                    return Ok(new { success = true, data = nft, txHash });
                }

                return BadRequest(new { success = false, message = "Invalid transfer" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error transferring NFT: {ex.Message}");
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

    public class CreateNFTRequest
    {
        public string DrugName { get; set; }
        public string BatchNumber { get; set; }
        public string ManufacturerAddress { get; set; }
        public string ManufacturerLocation { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public IFormFile DrugImage { get; set; }
        public IFormFile Certificate { get; set; }
    }

    public class UpdateNFTRequest
    {
        public string Status { get; set; }
    }
}
