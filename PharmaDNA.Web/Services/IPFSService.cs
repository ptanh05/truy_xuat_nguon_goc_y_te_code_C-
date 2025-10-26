using Newtonsoft.Json;
using PharmaDNA.Web.Models.ViewModels;

namespace PharmaDNA.Web.Services
{
    public class IPFSService : IIPFSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _pinataJwt;
        private readonly string _gatewayUrl;
        private readonly ILogger<IPFSService> _logger;

        public IPFSService(HttpClient httpClient, IConfiguration configuration, ILogger<IPFSService> logger)
        {
            _httpClient = httpClient;
            _pinataJwt = configuration["IPFS:PinataJWT"] ?? throw new InvalidOperationException("PinataJWT not configured");
            _gatewayUrl = configuration["IPFS:GatewayUrl"] ?? "https://gateway.pinata.cloud/ipfs/";
            _logger = logger;
        }

        public async Task<string> UploadMetadataAsync(ManufacturerViewModel model)
        {
            try
            {
                var metadata = new
                {
                    drugName = model.DrugName,
                    batchNumber = model.BatchNumber,
                    manufacturingDate = model.ManufacturingDate.ToString("yyyy-MM-dd"),
                    expiryDate = model.ExpiryDate.ToString("yyyy-MM-dd"),
                    description = model.Description,
                    manufacturerAddress = model.ManufacturerAddress,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    files = new List<string>(),
                    version = "1.0"
                };

                var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var formData = new MultipartFormDataContent();
                formData.Add(content, "file", "metadata.json");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pinata.cloud/pinning/pinFileToIPFS")
                {
                    Content = formData
                };
                request.Headers.Add("Authorization", $"Bearer {_pinataJwt}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var pinataResponse = JsonConvert.DeserializeObject<dynamic>(result);
                var ipfsHash = pinataResponse?.IpfsHash?.ToString();

                if (string.IsNullOrEmpty(ipfsHash))
                {
                    throw new InvalidOperationException("Failed to get IPFS hash from Pinata response");
                }

                _logger.LogInformation($"Metadata uploaded to IPFS: {ipfsHash}");
                return ipfsHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading metadata to IPFS");
                throw;
            }
        }

        public async Task<string> UploadSensorDataAsync(IFormFile sensorFile)
        {
            try
            {
                using var stream = sensorFile.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var formData = new MultipartFormDataContent();
                formData.Add(content, "file", sensorFile.FileName);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pinata.cloud/pinning/pinFileToIPFS")
                {
                    Content = formData
                };
                request.Headers.Add("Authorization", $"Bearer {_pinataJwt}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var pinataResponse = JsonConvert.DeserializeObject<dynamic>(result);
                var ipfsHash = pinataResponse?.IpfsHash?.ToString();

                if (string.IsNullOrEmpty(ipfsHash))
                {
                    throw new InvalidOperationException("Failed to get IPFS hash from Pinata response");
                }

                _logger.LogInformation($"Sensor data uploaded to IPFS: {ipfsHash}");
                return ipfsHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading sensor data to IPFS");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                var formData = new MultipartFormDataContent();
                formData.Add(content, "file", file.FileName);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pinata.cloud/pinning/pinFileToIPFS")
                {
                    Content = formData
                };
                request.Headers.Add("Authorization", $"Bearer {_pinataJwt}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var pinataResponse = JsonConvert.DeserializeObject<dynamic>(result);
                var ipfsHash = pinataResponse?.IpfsHash?.ToString();

                if (string.IsNullOrEmpty(ipfsHash))
                {
                    throw new InvalidOperationException("Failed to get IPFS hash from Pinata response");
                }

                _logger.LogInformation($"File uploaded to IPFS: {ipfsHash}");
                return ipfsHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to IPFS");
                throw;
            }
        }

        public string GetFileUrlAsync(string hash)
        {
            return $"{_gatewayUrl}{hash}";
        }
    }
}
