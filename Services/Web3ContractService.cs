using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Numerics;

namespace PharmaDNA.Services
{
    public class Web3ContractService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Web3ContractService> _logger;
        private readonly Web3 _web3;
        private readonly Contract _contract;
        private readonly string _contractAddress;
        private readonly string _contractABI;

        public Web3ContractService(IConfiguration configuration, ILogger<Web3ContractService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var rpcUrl = configuration["Blockchain:RpcUrl"];
            _contractAddress = configuration["Blockchain:ContractAddress"];
            _web3 = new Web3(rpcUrl);

            // PharmaNFT Contract ABI
            _contractABI = @"[
                {
                    ""constant"": false,
                    ""inputs"": [{""name"": ""uri"", ""type"": ""string""}],
                    ""name"": ""mintProductNFT"",
                    ""outputs"": [{""name"": """", ""type"": ""uint256""}],
                    ""type"": ""function""
                },
                {
                    ""constant"": false,
                    ""inputs"": [{""name"": ""to"", ""type"": ""address""}, {""name"": ""tokenId"", ""type"": ""uint256""}],
                    ""name"": ""transferProductNFT"",
                    ""outputs"": [],
                    ""type"": ""function""
                },
                {
                    ""constant"": true,
                    ""inputs"": [{""name"": ""tokenId"", ""type"": ""uint256""}],
                    ""name"": ""ownerOf"",
                    ""outputs"": [{""name"": """", ""type"": ""address""}],
                    ""type"": ""function""
                },
                {
                    ""constant"": true,
                    ""inputs"": [{""name"": ""account"", ""type"": ""address""}],
                    ""name"": ""getRole"",
                    ""outputs"": [{""name"": """", ""type"": ""uint8""}],
                    ""type"": ""function""
                },
                {
                    ""constant"": true,
                    ""inputs"": [{""name"": ""tokenId"", ""type"": ""uint256""}],
                    ""name"": ""getProductHistory"",
                    ""outputs"": [{""name"": """", ""type"": ""address[]""}],
                    ""type"": ""function""
                }
            ]";

            _contract = _web3.Eth.GetContract(_contractABI, _contractAddress);
        }

        public async Task<string> MintNFTAsync(string metadataUri, string manufacturerAddress)
        {
            try
            {
                _logger.LogInformation($"Minting NFT with URI: {metadataUri}");

                var mintFunction = _contract.GetFunction("mintProductNFT");
                var txHash = await mintFunction.SendTransactionAsync(
                    manufacturerAddress,
                    new Nethereum.Hex.HexTypes.HexBigInteger(0),
                    new Nethereum.Hex.HexTypes.HexBigInteger(300000),
                    metadataUri);

                _logger.LogInformation($"Mint transaction hash: {txHash}");
                return txHash;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error minting NFT: {ex.Message}");
                throw;
            }
        }

        public async Task<string> TransferNFTAsync(BigInteger tokenId, string toAddress, string fromAddress)
        {
            try
            {
                _logger.LogInformation($"Transferring NFT {tokenId} to {toAddress}");

                var transferFunction = _contract.GetFunction("transferProductNFT");
                var txHash = await transferFunction.SendTransactionAsync(
                    fromAddress,
                    new Nethereum.Hex.HexTypes.HexBigInteger(0),
                    new Nethereum.Hex.HexTypes.HexBigInteger(300000),
                    toAddress,
                    tokenId);

                _logger.LogInformation($"Transfer transaction hash: {txHash}");
                return txHash;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error transferring NFT: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetNFTOwnerAsync(BigInteger tokenId)
        {
            try
            {
                var ownerOfFunction = _contract.GetFunction("ownerOf");
                var owner = await ownerOfFunction.CallAsync<string>(tokenId);
                return owner;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFT owner: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetUserRoleAsync(string walletAddress)
        {
            try
            {
                var getRoleFunction = _contract.GetFunction("getRole");
                var role = await getRoleFunction.CallAsync<int>(walletAddress);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user role: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetProductHistoryAsync(BigInteger tokenId)
        {
            try
            {
                var historyFunction = _contract.GetFunction("getProductHistory");
                var history = await historyFunction.CallAsync<List<string>>(tokenId);
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product history: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyTransactionAsync(string txHash)
        {
            try
            {
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                return receipt != null && receipt.Status.Value == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying transaction: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetTransactionStatusAsync(string txHash)
        {
            try
            {
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                if (receipt == null)
                    return "pending";
                return receipt.Status.Value == 1 ? "confirmed" : "failed";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transaction status: {ex.Message}");
                return "unknown";
            }
        }
    }
}
