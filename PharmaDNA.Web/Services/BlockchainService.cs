using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;

namespace PharmaDNA.Web.Services
{
    public class BlockchainService : IBlockchainService
    {
        private readonly Web3 _web3;
        private readonly Contract _contract;
        private readonly string _contractAddress;
        private readonly string _privateKey;
        private readonly ILogger<BlockchainService> _logger;
        private readonly int _maxRetries = 3;
        private readonly int _retryDelayMs = 1000;

        public BlockchainService(IConfiguration configuration, ILogger<BlockchainService> logger)
        {
            var rpcUrl = Environment.GetEnvironmentVariable("PHARMADNA_RPC") 
                ?? configuration["Blockchain:RpcUrl"];
            var contractAddress = Environment.GetEnvironmentVariable("PHARMA_NFT_ADDRESS") 
                ?? configuration["Blockchain:ContractAddress"];
            var privateKey = Environment.GetEnvironmentVariable("OWNER_PRIVATE_KEY") 
                ?? configuration["Blockchain:PrivateKey"];
            
            if (string.IsNullOrEmpty(rpcUrl) || string.IsNullOrEmpty(contractAddress) || string.IsNullOrEmpty(privateKey))
            {
                throw new InvalidOperationException("Blockchain configuration is incomplete. Please check your .env file.");
            }
            
            _web3 = new Web3(rpcUrl);
            _contractAddress = contractAddress;
            _privateKey = privateKey;
            _logger = logger;
            
            // Load contract ABI and create contract instance
            var abi = LoadContractABI();
            _contract = _web3.Eth.GetContract(abi, contractAddress);
        }

        public async Task<bool> AssignRoleAsync(string address, int role)
        {
            try
            {
                var function = _contract.GetFunction("assignRole");
                var gas = await function.EstimateGasAsync(address, role);
                var transactionHash = await function.SendTransactionAsync(_privateKey, gas, null, address, role);
                _logger.LogInformation($"Role assigned: {address} -> {role}, TX: {transactionHash}");
                return !string.IsNullOrEmpty(transactionHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role to {address}");
                return false;
            }
        }

        public async Task<int> GetRoleAsync(string address)
        {
            try
            {
                var function = _contract.GetFunction("roles");
                var result = await function.CallAsync<int>(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting role for {address}");
                return 0;
            }
        }

        public async Task<string> MintNFTAsync(string ipfsHash, string manufacturerAddress)
        {
            try
            {
                var function = _contract.GetFunction("mintProductNFT");
                var gas = await function.EstimateGasAsync(ipfsHash);
                var transactionHash = await function.SendTransactionAsync(_privateKey, gas, null, ipfsHash);
                _logger.LogInformation($"NFT minted: {ipfsHash}, TX: {transactionHash}");
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error minting NFT with hash {ipfsHash}");
                return string.Empty;
            }
        }

        public async Task<bool> TransferNFTAsync(int tokenId, string fromAddress, string toAddress)
        {
            try
            {
                var function = _contract.GetFunction("transferProductNFT");
                var gas = await function.EstimateGasAsync(tokenId, toAddress);
                var transactionHash = await function.SendTransactionAsync(_privateKey, gas, null, tokenId, toAddress);
                _logger.LogInformation($"NFT transferred: {tokenId} from {fromAddress} to {toAddress}, TX: {transactionHash}");
                return !string.IsNullOrEmpty(transactionHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error transferring NFT {tokenId}");
                return false;
            }
        }

        public async Task<List<string>> GetProductHistoryAsync(int tokenId)
        {
            try
            {
                var function = _contract.GetFunction("getProductHistory");
                var result = await function.CallAsync<List<string>>(tokenId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product history for {tokenId}");
                return new List<string>();
            }
        }

        public async Task<string> GetProductCurrentOwnerAsync(int tokenId)
        {
            try
            {
                var function = _contract.GetFunction("getProductCurrentOwner");
                var result = await function.CallAsync<string>(tokenId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting current owner for {tokenId}");
                return string.Empty;
            }
        }

        public async Task<bool> HasRoleAsync(string address, int role)
        {
            try
            {
                var function = _contract.GetFunction("hasRole");
                var result = await function.CallAsync<bool>(address, role);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking role for {address}");
                return false;
            }
        }

        private string LoadContractABI()
        {
            try
            {
                var abiPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contracts", "PharmaNFT.json");
                if (File.Exists(abiPath))
                {
                    return File.ReadAllText(abiPath);
                }
                
                // Fallback to embedded resource or default ABI
                _logger.LogWarning("Contract ABI file not found, using default");
                return GetDefaultABI();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contract ABI");
                return GetDefaultABI();
            }
        }

        private string GetDefaultABI()
        {
            // Load ABI from environment variable or file
            var abiFromEnv = Environment.GetEnvironmentVariable("PHARMA_NFT_ABI");
            if (!string.IsNullOrEmpty(abiFromEnv))
            {
                return abiFromEnv;
            }

            // Fallback to empty ABI - will be loaded from file
            return "[]";
        }
    }
}
