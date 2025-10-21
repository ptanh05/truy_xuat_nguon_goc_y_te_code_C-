using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IBlockchainService
    {
        Task<string> MintNFTAsync(NFT nft, string metadataUri);
        Task<string> TransferNFTAsync(int tokenId, string toAddress);
        Task<string> GetNFTOwnerAsync(int tokenId);
        Task<bool> VerifyRoleAsync(string walletAddress, string role);
        Task<List<string>> GetProductHistoryAsync(int tokenId);
    }
}
