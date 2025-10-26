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
        public async Task<IActionResult> CreateNFT(ManufacturerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Upload to IPFS
                var ipfsHash = await _ipfsService.UploadMetadataAsync(model);
                
                // Save to database
                var nftDto = new NFTDto
                {
                    Name = model.DrugName,
                    BatchNumber = model.BatchNumber,
                    Status = "CREATED",
                    ManufacturerAddress = model.ManufacturerAddress,
                    IpfsHash = ipfsHash,
                    ManufactureDate = model.ManufacturingDate,
                    ExpiryDate = model.ExpiryDate,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow
                };
                
                var nftId = await _nftService.CreateNFTAsync(nftDto);
                
                // Mint NFT on blockchain
                var transactionHash = await _blockchainService.MintNFTAsync(ipfsHash, model.ManufacturerAddress);
                
                return Json(new { 
                    success = true, 
                    nftId, 
                    ipfsHash, 
                    transactionHash,
                    message = "NFT đã được tạo thành công!"
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
        public async Task<IActionResult> ApproveTransfer(int requestId, int nftId, string distributorAddress)
        {
            try
            {
                var success = await _nftService.ApproveTransferRequestAsync(requestId, nftId, distributorAddress);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving transfer request {requestId}");
                return Json(new { success = false, error = ex.Message });
            }
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
