using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Services;
using Newtonsoft.Json;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IPFSController : ControllerBase
    {
        private readonly IPinataService _pinataService;
        private readonly ILogger<IPFSController> _logger;

        public IPFSController(IPinataService pinataService, ILogger<IPFSController> logger)
        {
            _pinataService = pinataService;
            _logger = logger;
        }

        [HttpPost("upload-metadata")]
        public async Task<IActionResult> UploadMetadata([FromBody] Dictionary<string, object> metadata)
        {
            try
            {
                _logger.LogInformation("Uploading metadata to IPFS");
                var ipfsHash = await _pinataService.UploadMetadataAsync(metadata);
                return Ok(new { success = true, ipfsHash, gateway = $"https://gateway.pinata.cloud/ipfs/{ipfsHash}" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading metadata: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string metadata = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file provided" });

                var metadataDict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(metadata))
                {
                    metadataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(metadata);
                }

                _logger.LogInformation($"Uploading file {file.FileName} to IPFS");
                var ipfsHash = await _pinataService.UploadFileAsync(file, metadataDict);
                return Ok(new { success = true, ipfsHash, gateway = $"https://gateway.pinata.cloud/ipfs/{ipfsHash}" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleFiles([FromForm] List<IFormFile> files, [FromForm] string metadata = null)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new { success = false, message = "No files provided" });

                var metadataDict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(metadata))
                {
                    metadataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(metadata);
                }

                _logger.LogInformation($"Uploading {files.Count} files to IPFS");
                var ipfsHash = await _pinataService.UploadMultipleFilesAsync(files, metadataDict);
                return Ok(new { success = true, ipfsHash, gateway = $"https://gateway.pinata.cloud/ipfs/{ipfsHash}" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading files: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("metadata/{ipfsHash}")]
        public async Task<IActionResult> GetMetadata(string ipfsHash)
        {
            try
            {
                _logger.LogInformation($"Retrieving metadata for {ipfsHash}");
                var metadata = await _pinataService.GetMetadataAsync<dynamic>(ipfsHash);
                return Ok(new { success = true, data = metadata });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving metadata: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("unpin/{ipfsHash}")]
        public async Task<IActionResult> UnpinFile(string ipfsHash)
        {
            try
            {
                _logger.LogInformation($"Unpinning {ipfsHash}");
                var result = await _pinataService.UnpinAsync(ipfsHash);
                return Ok(new { success = result, message = result ? "File unpinned successfully" : "Failed to unpin file" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unpinning file: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("usage")]
        public async Task<IActionResult> GetUsage()
        {
            try
            {
                _logger.LogInformation("Getting Pinata usage");
                var usage = await _pinataService.GetUsageAsync();
                return Ok(new { success = true, data = usage });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting usage: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
