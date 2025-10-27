using PharmaDNA.Web.Models.DTOs;
using PharmaDNA.Web.Models.ViewModels;

namespace PharmaDNA.Web.Services
{
    public interface INFTService
    {
        Task<int> CreateNFTAsync(NFTDto nftDto);
        Task<NFTDto?> GetNFTByIdAsync(int id);
        Task<NFTDto?> GetNFTByBatchNumberAsync(string batchNumber);
        Task<List<NFTDto>> GetNFTsByStatusAsync(string status);
        Task<List<NFTDto>> GetAllNFTsAsync();
        Task<bool> UpdateNFTStatusAsync(int id, string status);
        Task<bool> UpdateNFTDistributorAsync(int id, string distributorAddress);
        Task<bool> UpdateNFTPharmacyAsync(int id, string pharmacyAddress);
        Task<int> CreateTransferRequestAsync(int nftId, string distributorAddress);
        Task<List<TransferRequestDto>> GetTransferRequestsAsync();
        Task<bool> ApproveTransferRequestAsync(int requestId, int nftId, string distributorAddress);
        Task<bool> AddMilestoneAsync(MilestoneViewModel milestone);
        Task<List<MilestoneInfo>> GetMilestonesByNftIdAsync(int nftId);
        Task<List<MilestoneInfo>> GetMilestonesByBatchNumberAsync(string batchNumber);
        Task<bool> UpdateSensorDataAsync(int nftId, string sensorIpfsHash);
        Task<bool> UpdateTransferRequestStatusAsync(int requestId, string status, DateTime updatedAt);
        Task<List<NFTDto>> GetNFTsByNameAsync(string name);
        Task<bool> DeleteTransferRequestAsync(int requestId, string distributorAddress);
    }
}
