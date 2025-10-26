namespace PharmaDNA.Web.Services
{
    public interface IBlockchainService
    {
        Task<bool> AssignRoleAsync(string address, int role);
        Task<int> GetRoleAsync(string address);
        Task<string> MintNFTAsync(string ipfsHash, string manufacturerAddress);
        Task<bool> TransferNFTAsync(int tokenId, string fromAddress, string toAddress);
        Task<List<string>> GetProductHistoryAsync(int tokenId);
        Task<string> GetProductCurrentOwnerAsync(int tokenId);
        Task<bool> HasRoleAsync(string address, int role);
    }
}
