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

        // Decode URL encoding first
        string cleaned;
        try
        {
            cleaned = Uri.UnescapeDataString(hash);
        }
        catch
        {
            // If decoding fails, use original hash
            cleaned = hash;
        }
        cleaned = cleaned.Trim()
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
            // Remove ipfs:// prefix
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

            // Handle mypinata.cloud URLs - extract the hash part
            if (cleaned.Contains("mypinata.cloud"))
            {
                // Try to extract hash after mypinata.cloud/
                var mypinataIndex = cleaned.IndexOf("mypinata.cloud", StringComparison.OrdinalIgnoreCase);
                if (mypinataIndex >= 0)
                {
                    var afterMypinata = cleaned.Substring(mypinataIndex + "mypinata.cloud".Length);
                    afterMypinata = afterMypinata.TrimStart('/');
                    // Extract hash (everything after the last /)
                    var lastSlash = afterMypinata.LastIndexOf('/');
                    if (lastSlash >= 0)
                    {
                        cleaned = afterMypinata.Substring(lastSlash + 1);
                    }
                    else
                    {
                        cleaned = afterMypinata;
                    }
                }
            }

            // Clean up any remaining URL parts
            cleaned = cleaned.Split('/').Last().Split('?').First().Split('#').First();

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

        // Validate that targetUrl is a valid absolute URI
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
        {
            _logger.LogError("Invalid target URL: {TargetUrl}", targetUrl);
            return BadRequest(new { error = "URL không hợp lệ", details = targetUrl });
        }
        targetUrl = uri.ToString();

        var gatewayToken = _configuration["PINATA_GATEWAY_TOKEN"];
        if (!string.IsNullOrWhiteSpace(gatewayToken))
        {
            var separator = targetUrl.Contains("?") ? "&" : "?";
            targetUrl = $"{targetUrl}{separator}pinataGatewayToken={gatewayToken}";
        }

        try
        {
            using var client = new HttpClient();
            // Ensure we're using an absolute URI
            if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var finalUri))
            {
                _logger.LogError("Invalid final URL: {TargetUrl}", targetUrl);
                return BadRequest(new { error = "URL không hợp lệ", details = targetUrl });
            }
            var response = await client.GetAsync(finalUri);
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
            var drugName = form["drugName"].ToString() ?? string.Empty;
            var batchNumber = form["batchNumber"].ToString() ?? string.Empty;
            var manufacturingDate = form["manufacturingDate"].ToString() ?? string.Empty;
            var expiryDate = form["expiryDate"].ToString() ?? string.Empty;
            var description = form["description"].ToString() ?? string.Empty;
            var gtin = form["gtin"].ToString() ?? string.Empty;
            var formulation = form["formulation"].ToString() ?? string.Empty;
            var manufacturerAddress = form["manufacturerAddress"].ToString() ?? string.Empty;
            var drugImage = form.Files["drugImage"];
            var certificate = form.Files["certificate"];

            _logger.LogInformation("Received upload request - Drug: {DrugName}, Batch: {BatchNumber}, Manufacturer: {Address}", 
                drugName, batchNumber, manufacturerAddress);

            if (string.IsNullOrEmpty(drugName) || string.IsNullOrEmpty(batchNumber) 
                || string.IsNullOrEmpty(manufacturingDate) || string.IsNullOrEmpty(expiryDate))
            {
                _logger.LogWarning("Missing required fields - DrugName: {DrugName}, BatchNumber: {BatchNumber}, ManufacturingDate: {ManufacturingDate}, ExpiryDate: {ExpiryDate}", 
                    drugName, batchNumber, manufacturingDate, expiryDate);
                return BadRequest(new { error = "Thiếu thông tin bắt buộc", details = new { drugName, batchNumber, manufacturingDate, expiryDate } });
            }

            if (string.IsNullOrEmpty(manufacturerAddress))
            {
                _logger.LogWarning("Missing manufacturer address");
                return BadRequest(new { error = "Thiếu địa chỉ ví manufacturer" });
            }

        var pinataJwt = _configuration["PINATA_JWT"];
        var pinataApiUrl = _configuration["PINATA_API_URL"] ?? "https://api.pinata.cloud";
        if (string.IsNullOrEmpty(pinataJwt))
        {
            _logger.LogError("PINATA_JWT chưa được cấu hình trong .env");
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
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(drugImage.ContentType ?? "application/octet-stream");

                    using var formData = new MultipartFormDataContent();
                    formData.Add(imageContent, "file", drugImage.FileName);

                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {pinataJwt}");
                    client.Timeout = TimeSpan.FromMinutes(5);

                    _logger.LogInformation("Uploading drug image to Pinata: {FileName}, Size: {Size} bytes", drugImage.FileName, drugImage.Length);
                    var response = await client.PostAsync($"{pinataApiUrl}/pinning/pinFileToIPFS", formData);
                    
                    var responseText = await response.Content.ReadAsStringAsync();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Dictionary<string, object>? result = null;
                        try
                        {
                            result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseText, new System.Text.Json.JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            });
                        }
                        catch (System.Text.Json.JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, "Failed to parse Pinata response as JSON. Response: {Response}", responseText);
                            throw new Exception($"Không thể parse response từ Pinata: {jsonEx.Message}");
                        }
                        
                        // Try both case variations
                        string? imageIpfsHash = null;
                        if (result != null)
                        {
                            if (result.TryGetValue("IpfsHash", out var hash1) && hash1 != null)
                                imageIpfsHash = hash1.ToString();
                            else if (result.TryGetValue("ipfsHash", out var hash2) && hash2 != null)
                                imageIpfsHash = hash2.ToString();
                            else if (result.TryGetValue("IPFSHash", out var hash3) && hash3 != null)
                                imageIpfsHash = hash3.ToString();
                        }
                        
                        if (!string.IsNullOrEmpty(imageIpfsHash))
                        {
                            uploadedFiles.Add($"ipfs/{imageIpfsHash}");
                            _logger.LogInformation("Successfully uploaded drug image to Pinata. Hash: {Hash}", imageIpfsHash);
                        }
                        else
                        {
                            _logger.LogWarning("Pinata response missing IPFS hash. Response: {Response}", responseText);
                            throw new Exception($"Pinata response không chứa IPFS hash. Response: {responseText.Substring(0, Math.Min(200, responseText.Length))}");
                        }
                    }
                    else
                    {
                        _logger.LogError("Pinata API error uploading drug image. Status: {Status}, Response: {Response}", response.StatusCode, responseText);
                        throw new Exception($"Lỗi khi upload hình ảnh lên Pinata: {response.StatusCode} - {responseText.Substring(0, Math.Min(500, responseText.Length))}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception uploading drug image to Pinata");
                    return StatusCode(500, new { error = "Lỗi khi upload hình ảnh lên Pinata", message = ex.Message });
                }
            }

            // Upload certificate if provided
            if (certificate != null && certificate.Length > 0)
            {
                try
                {
                    using var certStream = certificate.OpenReadStream();
                    using var certContent = new StreamContent(certStream);
                    certContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(certificate.ContentType ?? "application/octet-stream");

                    using var formData = new MultipartFormDataContent();
                    formData.Add(certContent, "file", certificate.FileName);

                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {pinataJwt}");
                    client.Timeout = TimeSpan.FromMinutes(5);

                    _logger.LogInformation("Uploading certificate to Pinata: {FileName}, Size: {Size} bytes", certificate.FileName, certificate.Length);
                    var response = await client.PostAsync($"{pinataApiUrl}/pinning/pinFileToIPFS", formData);
                    
                    var responseText = await response.Content.ReadAsStringAsync();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Dictionary<string, object>? result = null;
                        try
                        {
                            result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseText, new System.Text.Json.JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            });
                        }
                        catch (System.Text.Json.JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, "Failed to parse Pinata certificate response as JSON. Response: {Response}", responseText);
                            throw new Exception($"Không thể parse response từ Pinata: {jsonEx.Message}");
                        }
                        
                        // Try both case variations
                        string? certIpfsHash = null;
                        if (result != null)
                        {
                            if (result.TryGetValue("IpfsHash", out var hash1) && hash1 != null)
                                certIpfsHash = hash1.ToString();
                            else if (result.TryGetValue("ipfsHash", out var hash2) && hash2 != null)
                                certIpfsHash = hash2.ToString();
                            else if (result.TryGetValue("IPFSHash", out var hash3) && hash3 != null)
                                certIpfsHash = hash3.ToString();
                        }
                        
                        if (!string.IsNullOrEmpty(certIpfsHash))
                        {
                            uploadedFiles.Add($"ipfs/{certIpfsHash}");
                            _logger.LogInformation("Successfully uploaded certificate to Pinata. Hash: {Hash}", certIpfsHash);
                        }
                        else
                        {
                            _logger.LogWarning("Pinata response missing IPFS hash. Response: {Response}", responseText);
                            throw new Exception($"Pinata response không chứa IPFS hash. Response: {responseText.Substring(0, Math.Min(200, responseText.Length))}");
                        }
                    }
                    else
                    {
                        _logger.LogError("Pinata API error uploading certificate. Status: {Status}, Response: {Response}", response.StatusCode, responseText);
                        throw new Exception($"Lỗi khi upload chứng chỉ lên Pinata: {response.StatusCode} - {responseText.Substring(0, Math.Min(500, responseText.Length))}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception uploading certificate to Pinata");
                    return StatusCode(500, new { error = "Lỗi khi upload chứng chỉ lên Pinata", message = ex.Message });
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
            metadataClient.DefaultRequestHeaders.Clear();
            metadataClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pinataJwt}");
            metadataClient.Timeout = TimeSpan.FromMinutes(5);

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
                },
                pinataOptions = new
                {
                    cidVersion = 1
                }
            });

            var metadataContent = new StringContent(metadataJson, System.Text.Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Uploading metadata to Pinata for drug: {DrugName}, Batch: {BatchNumber}", drugName, batchNumber);
            var metadataResponse = await metadataClient.PostAsync($"{pinataApiUrl}/pinning/pinJSONToIPFS", metadataContent);
            
            var metadataResponseText = await metadataResponse.Content.ReadAsStringAsync();
            
            if (!metadataResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Pinata API error uploading metadata. Status: {Status}, Response: {Response}", metadataResponse.StatusCode, metadataResponseText);
                return StatusCode(500, new { error = "Lỗi khi upload metadata lên IPFS", details = metadataResponseText.Substring(0, Math.Min(500, metadataResponseText.Length)) });
            }

            Dictionary<string, object>? metadataResult = null;
            try
            {
                metadataResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(metadataResponseText, new System.Text.Json.JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to parse Pinata metadata response as JSON. Response: {Response}", metadataResponseText);
                return StatusCode(500, new { error = "Không thể parse response từ Pinata", details = jsonEx.Message });
            }
            
            // Try both case variations
            string? ipfsHash = null;
            if (metadataResult != null)
            {
                if (metadataResult.TryGetValue("IpfsHash", out var hash1) && hash1 != null)
                    ipfsHash = hash1.ToString();
                else if (metadataResult.TryGetValue("ipfsHash", out var hash2) && hash2 != null)
                    ipfsHash = hash2.ToString();
                else if (metadataResult.TryGetValue("IPFSHash", out var hash3) && hash3 != null)
                    ipfsHash = hash3.ToString();
            }

            if (string.IsNullOrEmpty(ipfsHash))
            {
                _logger.LogError("Pinata metadata response missing IPFS hash. Response: {Response}", metadataResponseText);
                return StatusCode(500, new { error = "Không thể lấy IPFS hash từ response", details = metadataResponseText.Substring(0, Math.Min(500, metadataResponseText.Length)) });
            }
            
            _logger.LogInformation("Successfully uploaded metadata to Pinata. Hash: {Hash}", ipfsHash);

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
            _logger.LogError(ex, "Error in UploadToIPFS: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            var errorResponse = new { 
                error = "Lỗi khi upload lên IPFS", 
                message = ex.Message
            };
            
            // Only include stack trace in development
            #if DEBUG
            return StatusCode(500, new { 
                error = "Lỗi khi upload lên IPFS", 
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.Message
            });
            #else
            return StatusCode(500, errorResponse);
            #endif
        }
    }
}

