using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Web.Models.ViewModels;
using PharmaDNA.Web.Services;
using PharmaDNA.Web.Models.DTOs;

namespace PharmaDNA.Web.Controllers
{
    public class ManufacturerController : Controller
    {
        private readonly INFTService _nftService;
        private readonly IBlockchainService _blockchainService;
        private readonly IIPFSService _ipfsService;
        private readonly ILogger<ManufacturerController> _logger;

        public ManufacturerController(
            INFTService nftService,
            IBlockchainService blockchainService,
            IIPFSService ipfsService,
            ILogger<ManufacturerController> logger)
        {
            _nftService = nftService;
            _blockchainService = blockchainService;
            _ipfsService = ipfsService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new ManufacturerViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNFT([FromForm] ManufacturerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Upload images and files to IPFS first
                var uploadedFiles = new List<string>();
                
                if (model.DrugImage != null && model.DrugImage.Length > 0)
                {
                    try
                    {
                        var imageHash = await _ipfsService.UploadFileAsync(model.DrugImage);
                        uploadedFiles.Add($"ipfs/{imageHash}");
                        _logger.LogInformation($"Drug image uploaded to IPFS: {imageHash}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading drug image");
                    }
                }

                if (model.Certificate != null && model.Certificate.Length > 0)
                {
                    try
                    {
                        var certHash = await _ipfsService.UploadFileAsync(model.Certificate);
                        uploadedFiles.Add($"ipfs/{certHash}");
                        _logger.LogInformation($"Certificate uploaded to IPFS: {certHash}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading certificate");
                    }
                }

                // Create metadata
                var metadata = new
                {
                    drugName = model.DrugName,
                    batchNumber = model.BatchNumber,
                    manufacturingDate = model.ManufacturingDate.ToString("yyyy-MM-dd"),
                    expiryDate = model.ExpiryDate.ToString("yyyy-MM-dd"),
                    description = model.Description,
                    manufacturerAddress = model.ManufacturerAddress,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    files = uploadedFiles,
                    version = "1.0"
                };

                // Upload metadata to IPFS
                var ipfsHash = await _ipfsService.UploadMetadataWithFilesAsync(model, model.DrugImage, model.Certificate);
                
                // Prepare image URLs
                var imageUrl = uploadedFiles.Count > 0
                    ? uploadedFiles[0].Replace("ipfs/", "https://gateway.pinata.cloud/ipfs/")
                    : null;
                
                var certificateUrl = uploadedFiles.Count > 1
                    ? uploadedFiles[1].Replace("ipfs/", "https://gateway.pinata.cloud/ipfs/")
                    : null;
                
                // Save to database
                var nftDto = new NFTDto
                {
                    Name = $"{model.DrugName} - {model.BatchNumber}",
                    BatchNumber = model.BatchNumber,
                    Status = "CREATED",
                    ManufacturerAddress = model.ManufacturerAddress,
                    IpfsHash = ipfsHash,
                    ImageUrl = imageUrl,
                    ManufactureDate = model.ManufacturingDate,
                    ExpiryDate = model.ExpiryDate,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow
                };
                
                var nftId = await _nftService.CreateNFTAsync(nftDto);
                
                return Json(new { 
                    success = true, 
                    IpfsHash = ipfsHash,
                    metadata = metadata,
                    filesUploaded = uploadedFiles.Count,
                    databaseId = nftId,
                    message = "Upload thành công và đã lưu vào database"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating NFT");
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTransferRequests()
        {
            try
            {
                var requests = await _nftService.GetTransferRequestsAsync();
                return Json(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer requests");
                return Json(new List<TransferRequestDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTransfer([FromBody] ApproveTransferRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RequestId.ToString()) || 
                    string.IsNullOrEmpty(request.NftId.ToString()) || 
                    string.IsNullOrEmpty(request.DistributorAddress))
                {
                    return Json(new { success = false, error = "Thiếu thông tin" });
                }

                // 1. Cập nhật trạng thái request sang 'approved'
                await _nftService.UpdateTransferRequestStatusAsync(
                    int.Parse(request.RequestId), 
                    "approved", 
                    DateTime.UtcNow
                );

                // 2. Cập nhật distributor_address và status cho NFT
                await _nftService.UpdateNFTDistributorAsync(
                    int.Parse(request.NftId), 
                    request.DistributorAddress
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving transfer request {request.RequestId}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        public class ApproveTransferRequest
        {
            public string RequestId { get; set; } = string.Empty;
            public string NftId { get; set; } = string.Empty;
            public string DistributorAddress { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetNFTs()
        {
            try
            {
                var nfts = await _nftService.GetAllNFTsAsync();
                return Json(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs");
                return Json(new List<NFTDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddMilestone(MilestoneViewModel model)
        {
            try
            {
                var success = await _nftService.AddMilestoneAsync(model);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding milestone");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMilestones(int nftId)
        {
            try
            {
                var milestones = await _nftService.GetMilestonesByNftIdAsync(nftId);
                return Json(milestones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting milestones for NFT {nftId}");
                return Json(new List<MilestoneInfo>());
            }
        }
    }
}
