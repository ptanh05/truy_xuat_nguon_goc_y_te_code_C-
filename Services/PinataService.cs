using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace PharmaDNA.Services
{
    public class PinataService : IPinataService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PinataService> _logger;
        private readonly HttpClient _httpClient;
        private const string PINATA_API_URL = "https://api.pinata.cloud";
        private const string PINATA_GATEWAY = "https://gateway.pinata.cloud/ipfs";

        public PinataService(IConfiguration configuration, ILogger<PinataService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        private void SetAuthHeaders()
        {
            var jwtToken = _configuration["Pinata:JwtToken"];
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        public async Task<string> UploadMetadataAsync(Dictionary<string, object> metadata)
        {
            try
            {
                _logger.LogInformation("Uploading metadata to Pinata");
                SetAuthHeaders();

                var jsonContent = JsonConvert.SerializeObject(metadata);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{PINATA_API_URL}/pinning/pinJSONToIPFS",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    var ipfsHash = result.IpfsHash;
                    _logger.LogInformation($"Metadata uploaded successfully: {ipfsHash}");
                    return ipfsHash;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Pinata error: {errorContent}");
                throw new Exception($"Failed to upload metadata to Pinata: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading metadata to Pinata: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, Dictionary<string, string> metadata)
        {
            try
            {
                _logger.LogInformation($"Uploading file {file.FileName} to Pinata");
                SetAuthHeaders();

                using (var content = new MultipartFormDataContent())
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var fileContent = new StreamContent(stream);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        content.Add(fileContent, "file", file.FileName);

                        // Add metadata
                        var metadataJson = JsonConvert.SerializeObject(new { keyvalues = metadata });
                        content.Add(new StringContent(metadataJson), "pinataMetadata");

                        var response = await _httpClient.PostAsync(
                            $"{PINATA_API_URL}/pinning/pinFileToIPFS",
                            content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            dynamic result = JsonConvert.DeserializeObject(responseContent);
                            var ipfsHash = result.IpfsHash;
                            _logger.LogInformation($"File uploaded successfully: {ipfsHash}");
                            return ipfsHash;
                        }

                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Pinata error: {errorContent}");
                        throw new Exception($"Failed to upload file to Pinata: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file to Pinata: {ex.Message}");
                throw;
            }
        }

        public async Task<T> GetMetadataAsync<T>(string ipfsHash)
        {
            try
            {
                _logger.LogInformation($"Retrieving metadata from IPFS: {ipfsHash}");

                var response = await _httpClient.GetAsync($"{PINATA_GATEWAY}/{ipfsHash}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<T>(content);
                    _logger.LogInformation($"Metadata retrieved successfully");
                    return result;
                }

                throw new Exception($"Failed to retrieve metadata from IPFS: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving metadata from IPFS: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadMultipleFilesAsync(List<IFormFile> files, Dictionary<string, string> metadata)
        {
            try
            {
                _logger.LogInformation($"Uploading {files.Count} files to Pinata");
                SetAuthHeaders();

                using (var content = new MultipartFormDataContent())
                {
                    foreach (var file in files)
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            var fileContent = new StreamContent(stream);
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                            content.Add(fileContent, "file", file.FileName);
                        }
                    }

                    // Add metadata
                    var metadataJson = JsonConvert.SerializeObject(new { keyvalues = metadata });
                    content.Add(new StringContent(metadataJson), "pinataMetadata");

                    var response = await _httpClient.PostAsync(
                        $"{PINATA_API_URL}/pinning/pinFileToIPFS",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseContent);
                        var ipfsHash = result.IpfsHash;
                        _logger.LogInformation($"Files uploaded successfully: {ipfsHash}");
                        return ipfsHash;
                    }

                    throw new Exception($"Failed to upload files to Pinata: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading files to Pinata: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UnpinAsync(string ipfsHash)
        {
            try
            {
                _logger.LogInformation($"Unpinning {ipfsHash} from Pinata");
                SetAuthHeaders();

                var response = await _httpClient.DeleteAsync(
                    $"{PINATA_API_URL}/pinning/unpin/{ipfsHash}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully unpinned {ipfsHash}");
                    return true;
                }

                throw new Exception($"Failed to unpin from Pinata: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unpinning from Pinata: {ex.Message}");
                throw;
            }
        }

        public async Task<PinataUsageResponse> GetUsageAsync()
        {
            try
            {
                _logger.LogInformation("Getting Pinata usage");
                SetAuthHeaders();

                var response = await _httpClient.GetAsync($"{PINATA_API_URL}/data/userPinnedDataTotal");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<PinataUsageResponse>(content);
                    return result;
                }

                throw new Exception($"Failed to get Pinata usage: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting Pinata usage: {ex.Message}");
                throw;
            }
        }
    }

    public class PinataUsageResponse
    {
        [JsonProperty("pin_count")]
        public int PinCount { get; set; }

        [JsonProperty("pin_size_total")]
        public long PinSizeTotal { get; set; }
    }
}
