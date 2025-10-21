using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface INFTService
    {
        Task<NFT> CreateNFTAsync(NFT nft);
        Task<NFT> GetNFTByIdAsync(int id);
        Task<NFT> GetNFTByBatchNumberAsync(string batchNumber);
        Task<List<NFT>> GetNFTsByManufacturerAsync(string manufacturerAddress);
        Task<List<NFT>> GetNFTsByDistributorAsync(string distributorAddress);
        Task<List<NFT>> GetNFTsByPharmacyAsync(string pharmacyAddress);
        Task<NFT> UpdateNFTAsync(NFT nft);
        Task<bool> DeleteNFTAsync(int id);
    }
}
