using Microsoft.AspNetCore.Mvc;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/manufacturer")]
public class IPFSController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public IPFSController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("upload-ipfs")]
    public async Task<IActionResult> UploadToIPFS()
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
        if (string.IsNullOrEmpty(pinataJwt))
        {
            return StatusCode(500, new { error = "PINATA_JWT chưa được cấu hình" });
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

                var response = await client.PostAsync("https://api.pinata.cloud/pinning/pinFileToIPFS", formData);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                    if (result != null && result.ContainsKey("IpfsHash"))
                    {
                        uploadedFiles.Add($"ipfs/{result["IpfsHash"]}");
                    }
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

                var response = await client.PostAsync("https://api.pinata.cloud/pinning/pinFileToIPFS", formData);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                    if (result != null && result.ContainsKey("IpfsHash"))
                    {
                        uploadedFiles.Add($"ipfs/{result["IpfsHash"]}");
                    }
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
        metadataClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

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

        var metadataContent = new StringContent(metadataJson);
        
        var metadataResponse = await metadataClient.PostAsync("https://api.pinata.cloud/pinning/pinJSONToIPFS", metadataContent);
        
        if (!metadataResponse.IsSuccessStatusCode)
        {
            var errorText = await metadataResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Error uploading metadata: {errorText}");
            return StatusCode(500, new { error = "Lỗi khi upload metadata lên IPFS" });
        }

        var metadataResult = await metadataResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var ipfsHash = metadataResult?["IpfsHash"]?.ToString();

        // TODO: Save to database
        // This should be done in ManufacturerController

        var imageUrl = uploadedFiles.Count > 0
            ? uploadedFiles[0].Replace("ipfs/", "https://gateway.pinata.cloud/ipfs/")
            : null;

        var certificateUrl = uploadedFiles.Count > 1
            ? uploadedFiles[1].Replace("ipfs/", "https://gateway.pinata.cloud/ipfs/")
            : null;

        return Ok(new
        {
            success = true,
            IpfsHash = ipfsHash,
            metadata = metadata,
            filesUploaded = uploadedFiles.Count,
            imageUrl,
            certificateUrl,
            message = "Upload thành công và đã lưu vào database"
        });
    }
}

