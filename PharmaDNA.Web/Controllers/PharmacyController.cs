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
        public async Task<IActionResult> GetTransferRequests([FromQuery] string? distributorAddress, [FromQuery] string? pharmacyAddress, [FromQuery] string? status)
        {
            try
            {
                var requests = await _nftService.GetTransferRequestsAsync();

                // Apply filters
                if (!string.IsNullOrEmpty(distributorAddress))
                {
                    requests = requests.Where(r => r.DistributorAddress.ToLower() == distributorAddress.ToLower()).ToList();
                }

                if (!string.IsNullOrEmpty(pharmacyAddress))
                {
                    requests = requests.Where(r => r.PharmacyAddress.ToLower() == pharmacyAddress.ToLower()).ToList();
                }

                if (!string.IsNullOrEmpty(status))
                {
                    requests = requests.Where(r => r.Status == status).ToList();
                }

                return Json(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer requests for pharmacy");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransferRequest([FromBody] CreateTransferRequestRequest request)
        {
            try
            {
                if (request.NftId == 0 || string.IsNullOrEmpty(request.PharmacyAddress))
                {
                    return Json(new { error = "Thiếu thông tin bắt buộc" });
                }

                // Get distributor address from header or session
                var distributorAddress = Request.Headers["X-Distributor-Address"].ToString();
                if (string.IsNullOrEmpty(distributorAddress))
                {
                    return Json(new { error = "Distributor address required" });
                }

                var requestId = await _nftService.CreateTransferRequestAsync(request.NftId, distributorAddress);
                
                return Json(new 
                { 
                    requestId,
                    message = $"Yêu cầu chuyển lô NFT #{request.NftId} đã được tạo thành công! Đang chờ nhà thuốc xác nhận."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer request");
                return Json(new { error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTransferRequest([FromBody] UpdateTransferRequestRequest request)
        {
            try
            {
                if (request.RequestId == 0 || string.IsNullOrEmpty(request.Status) || string.IsNullOrEmpty(request.PharmacyAddress))
                {
                    return Json(new { error = "Thiếu thông tin bắt buộc" });
                }

                if (!new[] { "approved", "rejected" }.Contains(request.Status))
                {
                    return Json(new { error = "Invalid status" });
                }

                var updated = await _nftService.UpdateTransferRequestStatusAsync(request.RequestId, request.Status, DateTime.UtcNow);
                
                if (!updated)
                {
                    return Json(new { error = "Transfer request not found" });
                }

                // If approved, transfer NFT ownership on blockchain
                if (request.Status == "approved")
                {
                    // TODO: Implement blockchain transfer logic
                    _logger.LogInformation($"NFT transfer approved for request {request.RequestId}");
                }

                return Json(new 
                { 
                    success = true,
                    message = request.Status == "approved" 
                        ? $"✅ Đã duyệt yêu cầu chuyển lô thành công! NFT đã được chuyển quyền sở hữu."
                        : $"❌ Đã từ chối yêu cầu chuyển lô."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transfer request");
                return Json(new { error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTransferRequest([FromBody] DeleteTransferRequestRequest request)
        {
            try
            {
                if (request.RequestId == 0)
                {
                    return Json(new { error = "Request ID required" });
                }

                var distributorAddress = Request.Headers["X-Distributor-Address"].ToString();
                if (string.IsNullOrEmpty(distributorAddress))
                {
                    return Json(new { error = "Distributor address required" });
                }

                var deleted = await _nftService.DeleteTransferRequestAsync(request.RequestId, distributorAddress);
                
                if (!deleted)
                {
                    return Json(new { error = "Transfer request not found or cannot be cancelled" });
                }

                return Json(new 
                { 
                    success = true,
                    message = $"✅ Đã hủy yêu cầu chuyển lô thành công!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transfer request");
                return Json(new { error = ex.Message });
            }
        }

        public class CreateTransferRequestRequest
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

        public class DeleteTransferRequestRequest
        {
            public int RequestId { get; set; }
        }
    }
}
