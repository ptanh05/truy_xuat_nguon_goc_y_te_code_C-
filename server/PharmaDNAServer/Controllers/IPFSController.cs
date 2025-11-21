using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;
using Npgsql;
using System.Net.Http;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/manufacturer")]
public class IPFSController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IPFSController> _logger;

    public IPFSController(IConfiguration configuration, ApplicationDbContext context, ILogger<IPFSController> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    [HttpGet("ipfs-file/{*hash}")]
    public async Task<IActionResult> ProxyIpfsFile(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return BadRequest(new { error = "Thiếu hash IPFS" });
        }

        var cleaned = hash.Trim()
            .Replace("\\", "")
            .Replace("\"", "")
            .Replace("'", "");

        bool isFullUrl =
            cleaned.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            cleaned.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        string targetUrl;

        if (isFullUrl)
        {
            targetUrl = cleaned;
        }
        else
        {
            cleaned = cleaned.Replace("ipfs://", "", StringComparison.OrdinalIgnoreCase)
                .Trim('/');

            if (cleaned.StartsWith("ipfs://", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring("ipfs://".Length);
            }
            if (cleaned.StartsWith("ipfs/", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring("ipfs/".Length);
            }

            if (cleaned.Contains("mypinata.cloud/"))
            {
                var parts = cleaned.Split(new[] { "mypinata.cloud/" }, StringSplitOptions.None);
                cleaned = parts[^1];
            }

            if (string.IsNullOrWhiteSpace(cleaned))
            {
                return BadRequest(new { error = "Hash IPFS không hợp lệ" });
            }

            var gateway = _configuration["PINATA_GATEWAY"];
            if (string.IsNullOrWhiteSpace(gateway))
            {
                gateway = "https://gateway.pinata.cloud/ipfs/";
            }
            if (!gateway.EndsWith("/"))
            {
                gateway += "/";
            }

            targetUrl = $"{gateway}{cleaned}";
        }

        var gatewayToken = _configuration["PINATA_GATEWAY_TOKEN"];
        if (!string.IsNullOrWhiteSpace(gatewayToken))
        {
            var separator = targetUrl.Contains("?") ? "&" : "?";
            targetUrl = $"{targetUrl}{separator}pinataGatewayToken={gatewayToken}";
        }

        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(targetUrl);
            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Không thể tải IPFS hash {Hash}. Status: {Status}. Details: {Details}", cleaned, response.StatusCode, details);
                return StatusCode((int)response.StatusCode, new { error = "Không tải được file từ IPFS", details });
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var stream = await response.Content.ReadAsStreamAsync();

            Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
            Response.Headers["Access-Control-Allow-Origin"] = "*";

            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi proxy IPFS hash {Hash}", cleaned);
            return StatusCode(500, new { error = "Không thể tải file từ IPFS", details = ex.Message });
        }
    }

    [HttpPost("upload-ipfs")]
    public async Task<IActionResult> UploadToIPFS()
    {
        try
        {
            var form = await Request.ReadFormAsync();
            var drugName = form["drugName"].ToString();
            var batchNumber = form["batchNumber"].ToString();
            var manufacturingDate = form["manufacturingDate"].ToString();
            var expiryDate = form["expiryDate"].ToString();
            var description = form["description"].ToString();
            var gtin = form["gtin"].ToString();
            var formulation = form["formulation"].ToString();
            var manufacturerAddress = form["manufacturerAddress"].ToString();
            var drugImage = form.Files["drugImage"];
            var certificate = form.Files["certificate"];

            if (string.IsNullOrEmpty(drugName) || string.IsNullOrEmpty(batchNumber) 
                || string.IsNullOrEmpty(manufacturingDate) || string.IsNullOrEmpty(expiryDate))
            {
                return BadRequest(new { error = "Thiếu thông tin bắt buộc" });
            }

            if (string.IsNullOrEmpty(manufacturerAddress))
            {
                return BadRequest(new { error = "Thiếu địa chỉ ví manufacturer" });
            }

        var pinataJwt = _configuration["PINATA_JWT"];
        var pinataApiUrl = _configuration["PINATA_API_URL"] ?? "https://api.pinata.cloud";
        if (string.IsNullOrEmpty(pinataJwt))
        {
            return StatusCode(500, new { error = "PINATA_JWT chưa được cấu hình trong .env" });
        }

            var uploadedFiles = new List<string>();

            // Upload drug image if provided
            if (drugImage != null && drugImage.Length > 0)
            {
                try
                {
                    using var imageStream = drugImage.OpenReadStream();
                    using var imageContent = new StreamContent(imageStream);

                    using var formData = new MultipartFormDataContent();
                    formData.Add(imageContent, "file", drugImage.FileName);

                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {pinataJwt}");

                    var response = await client.PostAsync($"{pinataApiUrl}/pinning/pinFileToIPFS", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                        if (result != null && result.ContainsKey("IpfsHash"))
                        {
                            uploadedFiles.Add($"ipfs/{result["IpfsHash"]}");
                        }
                    }
                    else
                    {
                        var errorText = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error uploading drug image to Pinata: {errorText}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading drug image: {ex.Message}");
                }
            }

            // Upload certificate if provided
            if (certificate != null && certificate.Length > 0)
            {
                try
                {
                    using var certStream = certificate.OpenReadStream();
                    using var certContent = new StreamContent(certStream);

                    using var formData = new MultipartFormDataContent();
                    formData.Add(certContent, "file", certificate.FileName);

                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {pinataJwt}");

                    var response = await client.PostAsync($"{pinataApiUrl}/pinning/pinFileToIPFS", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                        if (result != null && result.ContainsKey("IpfsHash"))
                        {
                            uploadedFiles.Add($"ipfs/{result["IpfsHash"]}");
                        }
                    }
                    else
                    {
                        var errorText = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error uploading certificate to Pinata: {errorText}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading certificate: {ex.Message}");
                }
            }

            // Create metadata JSON
            var metadata = new
            {
                drugName,
                batchNumber,
                gtin,
                manufacturingDate,
                expiryDate,
                description,
                formulation,
                manufacturerAddress,
                timestamp = DateTime.UtcNow.ToString("O"),
                files = uploadedFiles,
                version = "1.0"
            };

            // Upload metadata to IPFS
            using var metadataClient = new HttpClient();
            metadataClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pinataJwt}");

            var metadataJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                pinataContent = metadata,
                pinataMetadata = new
                {
                    name = $"{drugName}-{batchNumber}-metadata",
                    keyvalues = new
                    {
                        drugName,
                        batchNumber,
                        type = "drug-metadata"
                    }
                }
            });

            var metadataContent = new StringContent(metadataJson, System.Text.Encoding.UTF8, "application/json");
            
            var metadataResponse = await metadataClient.PostAsync($"{pinataApiUrl}/pinning/pinJSONToIPFS", metadataContent);
            
            if (!metadataResponse.IsSuccessStatusCode)
            {
                var errorText = await metadataResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error uploading metadata: {errorText}");
                return StatusCode(500, new { error = "Lỗi khi upload metadata lên IPFS", details = errorText });
            }

            var metadataResult = await metadataResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var ipfsHash = metadataResult?["IpfsHash"]?.ToString();

            if (string.IsNullOrEmpty(ipfsHash))
            {
                return StatusCode(500, new { error = "Không thể lấy IPFS hash từ response" });
            }

            // Lấy gateway URL, mặc định là Pinata public gateway
            var pinataGateway = _configuration["PINATA_GATEWAY"];
            if (string.IsNullOrWhiteSpace(pinataGateway))
            {
                pinataGateway = "https://gateway.pinata.cloud/ipfs/";
            }
            // Đảm bảo gateway URL kết thúc bằng /
            if (!pinataGateway.EndsWith("/"))
            {
                pinataGateway += "/";
            }

            var imageUrl = uploadedFiles.Count > 0
                ? uploadedFiles[0].Replace("ipfs/", pinataGateway)
                : null;

            var certificateUrl = uploadedFiles.Count > 1
                ? uploadedFiles[1].Replace("ipfs/", pinataGateway)
                : null;

            // Kiểm tra batch number đã tồn tại chưa
            var existingNft = await _context.NFTs
                .FirstOrDefaultAsync(n => n.BatchNumber == batchNumber);
            if (existingNft != null)
            {
                return BadRequest(new { error = $"Số lô {batchNumber} đã tồn tại trong hệ thống" });
            }

            // Lưu NFT vào database
            var now = DateTime.UtcNow;
            DateTime? ParseAsUtc(string input)
            {
                if (DateTime.TryParse(input, out var parsed))
                {
                    return parsed.Kind == DateTimeKind.Utc
                        ? parsed
                        : DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
                }
                return null;
            }

            var nft = new NFT
            {
                Name = drugName,
                BatchNumber = batchNumber,
                Gtin = string.IsNullOrWhiteSpace(gtin) ? null : gtin,
                ManufactureDate = ParseAsUtc(manufacturingDate),
                ExpiryDate = ParseAsUtc(expiryDate),
                Description = description,
                Formulation = string.IsNullOrWhiteSpace(formulation) ? null : formulation,
                ImageUrl = imageUrl,
                CertificateUrl = certificateUrl,
                Status = "created",
                IpfsHash = ipfsHash,
                ManufacturerAddress = manufacturerAddress.ToLower(),
                CreatedAt = now
            };

            try
            {
                _context.NFTs.Add(nft);
                await _context.SaveChangesAsync();
            }
        catch (DbUpdateException dbEx)
            {
            var innerMessage = dbEx.InnerException?.Message;
            _logger.LogError(dbEx, "Error saving NFT to database. Inner: {InnerMessage}", innerMessage);

            // Detect missing column (schema not migrated)
            if (dbEx.InnerException is PostgresException pgEx && pgEx.SqlState == "42703")
            {
                return StatusCode(500, new
                {
                    error = "Lỗi khi lưu NFT vào database",
                    message = "Schema database chưa đồng bộ (thiếu cột). Hãy chạy dotnet ef database update.",
                    innerMessage = innerMessage,
                    ipfsHash = ipfsHash
                });
            }

                Console.WriteLine($"Error saving NFT to database: {dbEx.Message}");
                return StatusCode(500, new { 
                    error = "Lỗi khi lưu NFT vào database", 
                message = dbEx.Message,
                innerMessage,
                ipfsHash = ipfsHash // Trả về IPFS hash để có thể retry
                });
            }

            return Ok(new
            {
                success = true,
                IpfsHash = ipfsHash,
                metadata = metadata,
                filesUploaded = uploadedFiles.Count,
                imageUrl,
                certificateUrl,
                databaseId = nft.Id,
                message = "Upload thành công và đã lưu vào database"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UploadToIPFS: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { 
                error = "Lỗi khi upload lên IPFS", 
                message = ex.Message 
            });
        }
    }
}

