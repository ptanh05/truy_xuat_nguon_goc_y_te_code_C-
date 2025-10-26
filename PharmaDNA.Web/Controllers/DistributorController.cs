using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Web.Models.ViewModels;
using PharmaDNA.Web.Services;

namespace PharmaDNA.Web.Controllers
{
    public class DistributorController : Controller
    {
        private readonly INFTService _nftService;
        private readonly IIPFSService _ipfsService;
        private readonly ILogger<DistributorController> _logger;

        public DistributorController(
            INFTService nftService, 
            IIPFSService ipfsService,
            ILogger<DistributorController> logger)
        {
            _nftService = nftService;
            _ipfsService = ipfsService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new DistributorViewModel();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetNFTs()
        {
            try
            {
                var nfts = await _nftService.GetNFTsByStatusAsync("in_transit");
                return Json(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs for distributor");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadSensorData(IFormFile sensorFile, int nftId, string distributorAddress)
        {
            try
            {
                if (sensorFile == null || sensorFile.Length == 0)
                {
                    return Json(new { success = false, error = "Không có file được chọn" });
                }

                var ipfsHash = await _ipfsService.UploadSensorDataAsync(sensorFile);
                var success = await _nftService.UpdateSensorDataAsync(nftId, ipfsHash);
                
                if (success)
                {
                    return Json(new { success = true, ipfsHash });
                }
                else
                {
                    return Json(new { success = false, error = "Không thể cập nhật dữ liệu cảm biến" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading sensor data");
                return Json(new { success = false, error = ex.Message });
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

        [HttpPost]
        public async Task<IActionResult> RequestTransfer(int nftId, string distributorAddress)
        {
            try
            {
                var requestId = await _nftService.CreateTransferRequestAsync(nftId, distributorAddress);
                return Json(new { success = true, requestId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer request");
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
