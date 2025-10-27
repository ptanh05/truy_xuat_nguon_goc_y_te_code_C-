using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Web.Services;
using PharmaDNA.Web.Models.DTOs;
using PharmaDNA.Web.Models.ViewModels;

namespace PharmaDNA.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly INFTService _nftService;
        private readonly IUserService _userService;
        private readonly IBlockchainService _blockchainService;
        private readonly IIPFSService _ipfsService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(
            INFTService nftService,
            IUserService userService,
            IBlockchainService blockchainService,
            IIPFSService ipfsService,
            ILogger<ApiController> logger)
        {
            _nftService = nftService;
            _userService = userService;
            _blockchainService = blockchainService;
            _ipfsService = ipfsService;
            _logger = logger;
        }

        /// <summary>
        /// Tra cứu thuốc bằng số lô (API cho mobile app)
        /// </summary>
        [HttpGet("lookup/{batchNumber}")]
        public async Task<ActionResult<ApiResponse<DrugLookupResult>>> LookupDrug(string batchNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(batchNumber))
                {
                    return BadRequest(ApiResponse<DrugLookupResult>.ErrorResponse("Số lô không được để trống"));
                }

                var nft = await _nftService.GetNFTByBatchNumberAsync(batchNumber);
                if (nft == null)
                {
                    return NotFound(ApiResponse<DrugLookupResult>.ErrorResponse("Không tìm thấy lô thuốc với số lô này"));
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

                return Ok(ApiResponse<DrugLookupResult>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up drug with batch number: {batchNumber}");
                return StatusCode(500, ApiResponse<DrugLookupResult>.ErrorResponse("Có lỗi xảy ra khi tra cứu thuốc"));
            }
        }

        /// <summary>
        /// Lấy danh sách NFTs theo role
        /// </summary>
        [HttpGet("nfts")]
        public async Task<ActionResult<ApiResponse<List<NFTDto>>>> GetNFTs([FromQuery] string? status, [FromQuery] string? role)
        {
            try
            {
                List<NFTDto> nfts;
                
                if (!string.IsNullOrEmpty(status))
                {
                    nfts = await _nftService.GetNFTsByStatusAsync(status);
                }
                else
                {
                    nfts = await _nftService.GetAllNFTsAsync();
                }

                return Ok(ApiResponse<List<NFTDto>>.SuccessResponse(nfts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs");
                return StatusCode(500, ApiResponse<List<NFTDto>>.ErrorResponse("Có lỗi xảy ra khi lấy danh sách NFTs"));
            }
        }

        /// <summary>
        /// Tạo NFT mới (cho Manufacturer)
        /// </summary>
        [HttpPost("nfts")]
        public async Task<ActionResult<ApiResponse<CreateNFTResponse>>> CreateNFT([FromBody] CreateNFTRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<CreateNFTResponse>.ErrorResponse("Dữ liệu không hợp lệ"));
                }

                // Upload metadata to IPFS
                var metadata = new
                {
                    drugName = request.DrugName,
                    batchNumber = request.BatchNumber,
                    manufacturingDate = request.ManufacturingDate.ToString("yyyy-MM-dd"),
                    expiryDate = request.ExpiryDate.ToString("yyyy-MM-dd"),
                    description = request.Description,
                    manufacturerAddress = request.ManufacturerAddress,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    version = "1.0"
                };

                var ipfsHash = await _ipfsService.UploadMetadataAsync(new ManufacturerViewModel
                {
                    DrugName = request.DrugName,
                    BatchNumber = request.BatchNumber,
                    ManufacturingDate = request.ManufacturingDate,
                    ExpiryDate = request.ExpiryDate,
                    Description = request.Description,
                    ManufacturerAddress = request.ManufacturerAddress
                });

                // Save to database
                var nftDto = new NFTDto
                {
                    Name = $"{request.DrugName} - {request.BatchNumber}",
                    BatchNumber = request.BatchNumber,
                    Status = "CREATED",
                    ManufacturerAddress = request.ManufacturerAddress,
                    IpfsHash = ipfsHash,
                    ManufactureDate = request.ManufacturingDate,
                    ExpiryDate = request.ExpiryDate,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow
                };
                
                var nftId = await _nftService.CreateNFTAsync(nftDto);

                var response = new CreateNFTResponse
                {
                    NftId = nftId,
                    IpfsHash = ipfsHash,
                    BatchNumber = request.BatchNumber
                };

                return Ok(ApiResponse<CreateNFTResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating NFT");
                return StatusCode(500, ApiResponse<CreateNFTResponse>.ErrorResponse("Có lỗi xảy ra khi tạo NFT"));
            }
        }

        /// <summary>
        /// Lấy lịch sử milestones của một NFT
        /// </summary>
        [HttpGet("nfts/{nftId}/milestones")]
        public async Task<ActionResult<ApiResponse<List<MilestoneInfo>>>> GetMilestones(int nftId)
        {
            try
            {
                var milestones = await _nftService.GetMilestonesByNftIdAsync(nftId);
                return Ok(ApiResponse<List<MilestoneInfo>>.SuccessResponse(milestones));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting milestones for NFT {nftId}");
                return StatusCode(500, ApiResponse<List<MilestoneInfo>>.ErrorResponse("Có lỗi xảy ra khi lấy lịch sử"));
            }
        }

        /// <summary>
        /// Thêm milestone mới
        /// </summary>
        [HttpPost("nfts/{nftId}/milestones")]
        public async Task<ActionResult<ApiResponse<bool>>> AddMilestone(int nftId, [FromBody] AddMilestoneRequest request)
        {
            try
            {
                var milestone = new MilestoneViewModel
                {
                    NftId = nftId,
                    Type = request.Type,
                    Description = request.Description,
                    Location = request.Location,
                    ActorAddress = request.ActorAddress
                };

                var success = await _nftService.AddMilestoneAsync(milestone);
                
                if (success)
                {
                    return Ok(ApiResponse<bool>.SuccessResponse(true));
                }
                else
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Không thể thêm milestone"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding milestone for NFT {nftId}");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Có lỗi xảy ra khi thêm milestone"));
            }
        }

        /// <summary>
        /// Lấy thông tin user theo địa chỉ
        /// </summary>
        [HttpGet("users/{address}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string address)
        {
            try
            {
                var user = await _userService.GetUserByAddressAsync(address);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResponse("Không tìm thấy user"));
                }

                return Ok(ApiResponse<UserDto>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user {address}");
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Có lỗi xảy ra khi lấy thông tin user"));
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public ActionResult<ApiResponse<HealthCheckResponse>> HealthCheck()
        {
            try
            {
                var response = new HealthCheckResponse
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0"
                };

                return Ok(ApiResponse<HealthCheckResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, ApiResponse<HealthCheckResponse>.ErrorResponse("Health check failed"));
            }
        }
    }

    // Request/Response DTOs
    public class CreateNFTRequest
    {
        public string DrugName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string ManufacturerAddress { get; set; } = string.Empty;
    }

    public class CreateNFTResponse
    {
        public int NftId { get; set; }
        public string IpfsHash { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
    }

    public class AddMilestoneRequest
    {
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string ActorAddress { get; set; } = string.Empty;
    }

    public class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
    }

    // Generic API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = error
            };
        }
    }
}
