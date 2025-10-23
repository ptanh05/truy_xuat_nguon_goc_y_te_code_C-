namespace PharmaDNA.Services
{
    public interface IPinataService
    {
        Task<string> UploadMetadataAsync(Dictionary<string, object> metadata);
        Task<string> UploadFileAsync(IFormFile file, Dictionary<string, string> metadata);
        Task<T> GetMetadataAsync<T>(string ipfsHash);
        Task<string> UploadMultipleFilesAsync(IEnumerable<IFormFile> files, Dictionary<string, string> metadata);
        Task<bool> UnpinAsync(string ipfsHash);
        Task<object> GetUsageAsync();
    }
}
