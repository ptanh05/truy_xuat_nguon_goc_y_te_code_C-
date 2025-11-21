using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/manufacturer")]
public class IPFSController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public IPFSController(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
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
                manufacturingDate,
                expiryDate,
                description,
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
            var nft = new NFT
            {
                Name = drugName,
                BatchNumber = batchNumber,
                ManufactureDate = DateTime.TryParse(manufacturingDate, out var mfgDate) ? mfgDate : null,
                ExpiryDate = DateTime.TryParse(expiryDate, out var expDate) ? expDate : null,
                Description = description,
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
            catch (Exception dbEx)
            {
                Console.WriteLine($"Error saving NFT to database: {dbEx.Message}");
                return StatusCode(500, new { 
                    error = "Lỗi khi lưu NFT vào database", 
                    message = dbEx.Message,
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

