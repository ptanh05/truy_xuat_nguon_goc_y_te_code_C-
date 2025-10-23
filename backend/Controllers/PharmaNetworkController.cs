using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Services;
using PharmaDNA.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PharmaNetworkController : ControllerBase
    {
        private readonly IPharmaNetworkService _pharmaNetworkService;

        public PharmaNetworkController(IPharmaNetworkService pharmaNetworkService)
        {
            _pharmaNetworkService = pharmaNetworkService;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetNetworkInfo()
        {
            var info = await _pharmaNetworkService.GetNetworkInfoAsync();
            return Ok(info);
        }

        [HttpPost("nft")]
        public async Task<IActionResult> CreateNFT([FromBody] CreateNFTRequest request)
        {
            try
            {
                var nft = new NFT
                {
                    ProductName = request.ProductName,
                    ProductCode = request.ProductCode,
                    BatchId = request.BatchId,
                    Manufacturer = request.Manufacturer,
                    ExpiryDate = request.ExpiryDate,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    ProductType = request.ProductType
                };

                var transactionHash = await _pharmaNetworkService.CreateNFTAsync(nft);
                
                return Ok(new { 
                    success = true, 
                    transactionHash,
                    nftId = nft.Id,
                    message = "NFT created successfully on Pharma Network"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = ex.Message 
                });
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferNFT([FromBody] TransferNFTRequest request)
        {
            try
            {
                var transactionHash = await _pharmaNetworkService.TransferNFTAsync(
                    request.NFTId, 
                    request.ToAddress, 
                    request.FromAddress
                );

                return Ok(new { 
                    success = true, 
                    transactionHash,
                    message = "Transfer initiated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = ex.Message 
                });
            }
        }

        [HttpGet("nft/{id}")]
        public async Task<IActionResult> GetNFT(int id)
        {
            var nft = await _pharmaNetworkService.GetNFTAsync(id);
            if (nft == null)
            {
                return NotFound(new { 
                    success = false, 
                    message = "NFT not found" 
                });
            }

            return Ok(new { 
                success = true, 
                nft 
            });
        }

        [HttpGet("nfts")]
        public async Task<IActionResult> GetAllNFTs()
        {
            var nfts = await _pharmaNetworkService.GetAllNFTsAsync();
            return Ok(new { 
                success = true, 
                nfts,
                count = nfts.Count
            });
        }

        [HttpGet("transaction/{hash}/status")]
        public async Task<IActionResult> GetTransactionStatus(string hash)
        {
            var status = await _pharmaNetworkService.GetTransactionStatusAsync(hash);
            return Ok(new { 
                success = true, 
                transactionHash = hash,
                status 
            });
        }

        [HttpGet("balance/{address}")]
        public async Task<IActionResult> GetBalance(string address)
        {
            var balance = await _pharmaNetworkService.GetBalanceAsync(address);
            return Ok(new { 
                success = true, 
                address,
                balance 
            });
        }
    }

    public class CreateNFTRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string BatchId { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ProductType { get; set; } = string.Empty;
    }

    public class TransferNFTRequest
    {
        public int NFTId { get; set; }
        public string ToAddress { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
    }
}
