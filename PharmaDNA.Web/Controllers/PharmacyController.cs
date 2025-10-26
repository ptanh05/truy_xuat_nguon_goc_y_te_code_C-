using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Web.Models.ViewModels;
using PharmaDNA.Web.Services;

namespace PharmaDNA.Web.Controllers
{
    public class PharmacyController : Controller
    {
        private readonly INFTService _nftService;
        private readonly ILogger<PharmacyController> _logger;

        public PharmacyController(INFTService nftService, ILogger<PharmacyController> logger)
        {
            _nftService = nftService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new PharmacyViewModel();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LookupDrug(string batchNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(batchNumber))
                {
                    return Json(new { success = false, error = "Số lô không được để trống" });
                }

                var nft = await _nftService.GetNFTByBatchNumberAsync(batchNumber);
                if (nft == null)
                {
                    return Json(new { success = false, error = "Không tìm thấy lô thuốc với số lô này" });
                }

                var milestones = await _nftService.GetMilestonesByBatchNumberAsync(batchNumber);
                
                var result = new DrugLookupResult
                {
                    Id = nft.Id,
                    Name = nft.Name,
                    BatchNumber = nft.BatchNumber,
                    Status = nft.Status,
                    ManufacturerAddress = nft.ManufacturerAddress,
                    DistributorAddress = nft.DistributorAddress,
                    PharmacyAddress = nft.PharmacyAddress,
                    ManufactureDate = nft.ManufactureDate,
                    ExpiryDate = nft.ExpiryDate,
                    Description = nft.Description,
                    ImageUrl = nft.ImageUrl,
                    Milestones = milestones
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up drug with batch number: {batchNumber}");
                return Json(new { success = false, error = "Có lỗi xảy ra khi tra cứu thuốc" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReceived(string batchNumber, string pharmacyAddress)
        {
            try
            {
                var nft = await _nftService.GetNFTByBatchNumberAsync(batchNumber);
                if (nft == null)
                {
                    return Json(new { success = false, error = "Không tìm thấy lô thuốc" });
                }

                // Add milestone for pharmacy confirmation
                var milestone = new MilestoneViewModel
                {
                    NftId = nft.Id,
                    Type = "Đã nhập kho",
                    Description = "Nhà thuốc xác nhận đã nhận lô thuốc",
                    ActorAddress = pharmacyAddress
                };

                var milestoneSuccess = await _nftService.AddMilestoneAsync(milestone);
                var statusSuccess = await _nftService.UpdateNFTPharmacyAsync(nft.Id, pharmacyAddress);

                if (milestoneSuccess && statusSuccess)
                {
                    return Json(new { success = true, message = "Đã xác nhận nhập kho thành công!" });
                }
                else
                {
                    return Json(new { success = false, error = "Không thể xác nhận nhập kho" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming received drug: {batchNumber}");
                return Json(new { success = false, error = "Có lỗi xảy ra khi xác nhận nhập kho" });
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
                _logger.LogError(ex, "Error getting transfer requests for pharmacy");
                return Json(new List<object>());
            }
        }
    }
}
