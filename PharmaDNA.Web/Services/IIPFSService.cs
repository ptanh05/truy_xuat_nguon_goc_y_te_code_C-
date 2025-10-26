using PharmaDNA.Web.Models.ViewModels;

namespace PharmaDNA.Web.Services
{
    public interface IIPFSService
    {
        Task<string> UploadMetadataAsync(ManufacturerViewModel model);
        Task<string> UploadSensorDataAsync(IFormFile sensorFile);
        Task<string> UploadFileAsync(IFormFile file);
        Task<string> GetFileUrlAsync(string hash);
    }
}
