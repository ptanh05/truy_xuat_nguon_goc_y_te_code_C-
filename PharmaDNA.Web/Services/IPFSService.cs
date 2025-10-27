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
            _pinataJwt = Environment.GetEnvironmentVariable("PINATA_JWT") 
                ?? throw new InvalidOperationException("PINATA_JWT environment variable not configured");
            _gatewayUrl = Environment.GetEnvironmentVariable("PINATA_GATEWAY") 
                ?? "https://gateway.pinata.cloud/ipfs/";
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

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pinata.cloud/pinning/pinJSONToIPFS")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        pinataContent = metadata,
                        pinataMetadata = new
                        {
                            name = $"{model.DrugName}-{model.BatchNumber}-metadata",
                            keyvalues = new
                            {
                                drugName = model.DrugName,
                                batchNumber = model.BatchNumber,
                                type = "drug-metadata"
                            }
                        }
                    }), System.Text.Encoding.UTF8, "application/json")
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

        public async Task<string> UploadMetadataWithFilesAsync(ManufacturerViewModel model, IFormFile? drugImage, IFormFile? certificate)
        {
            try
            {
                var uploadedFiles = new List<string>();

                // Upload drug image if provided
                if (drugImage != null && drugImage.Length > 0)
                {
                    try
                    {
                        var imageHash = await UploadFileAsync(drugImage);
                        uploadedFiles.Add($"ipfs/{imageHash}");
                        _logger.LogInformation($"Drug image uploaded to IPFS: {imageHash}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading drug image");
                    }
                }

                // Upload certificate if provided
                if (certificate != null && certificate.Length > 0)
                {
                    try
                    {
                        var certHash = await UploadFileAsync(certificate);
                        uploadedFiles.Add($"ipfs/{certHash}");
                        _logger.LogInformation($"Certificate uploaded to IPFS: {certHash}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading certificate");
                    }
                }

                // Create metadata
                var metadata = new
                {
                    drugName = model.DrugName,
                    batchNumber = model.BatchNumber,
                    manufacturingDate = model.ManufacturingDate.ToString("yyyy-MM-dd"),
                    expiryDate = model.ExpiryDate.ToString("yyyy-MM-dd"),
                    description = model.Description,
                    manufacturerAddress = model.ManufacturerAddress,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    files = uploadedFiles,
                    version = "1.0"
                };

                // Upload metadata to IPFS
                return await UploadMetadataAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading metadata with files to IPFS");
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

        public Task<string> GetFileUrlAsync(string hash)
        {
            return Task.FromResult($"{_gatewayUrl}{hash}");
        }

        public async Task<bool> VerifyIPFSHashAsync(string hash)
        {
            try
            {
                var url = await GetFileUrlAsync(hash);
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
