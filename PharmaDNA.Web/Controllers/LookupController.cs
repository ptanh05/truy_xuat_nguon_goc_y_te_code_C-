using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Web.Models.ViewModels;
using PharmaDNA.Web.Services;

namespace PharmaDNA.Web.Controllers
{
    public class LookupController : Controller
    {
        private readonly INFTService _nftService;
        private readonly ILogger<LookupController> _logger;

        public LookupController(INFTService nftService, ILogger<LookupController> logger)
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
        public async Task<IActionResult> Search(string batchNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(batchNumber))
                {
                    return Json(new { success = false, error = "Vui lòng nhập số lô thuốc" });
                }

                var nft = await _nftService.GetNFTByBatchNumberAsync(batchNumber);
                if (nft == null)
                {
                    return Json(new { 
                        success = false, 
                        error = "Không tìm thấy lô thuốc với số lô này" 
                    });
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
                _logger.LogError(ex, $"Error searching for batch number: {batchNumber}");
                return Json(new { 
                    success = false, 
                    error = "Có lỗi xảy ra khi tra cứu thuốc" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDrugHistory(string batchNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(batchNumber))
                {
                    return Json(new { success = false, error = "Vui lòng nhập số lô thuốc" });
                }

                var milestones = await _nftService.GetMilestonesByBatchNumberAsync(batchNumber);
                
                return Json(new { success = true, milestones });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting drug history for batch: {batchNumber}");
                return Json(new { 
                    success = false, 
                    error = "Có lỗi xảy ra khi lấy lịch sử thuốc" 
                });
            }
        }
    }
}
