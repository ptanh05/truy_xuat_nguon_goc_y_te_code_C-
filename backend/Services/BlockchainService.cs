using Nethereum.Web3;
using Nethereum.Contracts;
using PharmaDNA.Models;
using System.Numerics;

namespace PharmaDNA.Services
{
    public class BlockchainService : IBlockchainService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlockchainService> _logger;
        private readonly Web3ContractService _web3ContractService;

        public BlockchainService(
            IConfiguration configuration, 
            ILogger<BlockchainService> logger,
            Web3ContractService web3ContractService)
        {
            _configuration = configuration;
            _logger = logger;
            _web3ContractService = web3ContractService;
        }

        public async Task<string> MintNFTAsync(Models.NFT nft, string metadataUri)
        {
            try
            {
                _logger.LogInformation($"Minting NFT for batch {nft.BatchNumber}");
                var txHash = await _web3ContractService.MintNFTAsync(metadataUri, nft.ManufacturerAddress);
                return txHash;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error minting NFT: {ex.Message}");
                throw;
            }
        }

        public async Task<string> TransferNFTAsync(int tokenId, string toAddress)
        {
            try
            {
                _logger.LogInformation($"Transferring NFT {tokenId} to {toAddress}");
                var txHash = await _web3ContractService.TransferNFTAsync(
                    new BigInteger(tokenId),
                    toAddress,
                    _configuration["Blockchain:OwnerAddress"]);
                return txHash;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error transferring NFT: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetNFTOwnerAsync(int tokenId)
        {
            try
            {
                _logger.LogInformation($"Getting owner of NFT {tokenId}");
                var owner = await _web3ContractService.GetNFTOwnerAsync(new BigInteger(tokenId));
                return owner;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFT owner: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyRoleAsync(string walletAddress, string role)
        {
            try
            {
                _logger.LogInformation($"Verifying role {role} for {walletAddress}");
                var userRole = await _web3ContractService.GetUserRoleAsync(walletAddress);
                // Role mapping: 0=None, 1=Manufacturer, 2=Distributor, 3=Pharmacy, 4=Admin
                var expectedRole = role switch
                {
                    "Manufacturer" => 1,
                    "Distributor" => 2,
                    "Pharmacy" => 3,
                    "Admin" => 4,
                    _ => 0
                };
                return userRole == expectedRole;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying role: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetProductHistoryAsync(int tokenId)
        {
            try
            {
                _logger.LogInformation($"Getting product history for NFT {tokenId}");
                var history = await _web3ContractService.GetProductHistoryAsync(new BigInteger(tokenId));
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product history: {ex.Message}");
                throw;
            }
        }
    }
}
