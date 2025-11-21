using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace PharmaDNAServer.Services;

public record OnChainTransactionResult(bool Success, string? TransactionHash, string? Error);

public class BlockchainService
{
    private readonly Models.ContractOptions _options;
    private readonly ILogger<BlockchainService> _logger;
    private static readonly IReadOnlyDictionary<string, int> RoleMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["MANUFACTURER"] = 1,
        ["DISTRIBUTOR"] = 2,
        ["PHARMACY"] = 3,
        ["ADMIN"] = 4,
    };

    // Minimal ABI for assignRole(address,uint8)
    private const string AssignRoleAbi = """
    [
        {
            "inputs": [
                { "internalType": "address", "name": "user", "type": "address" },
                { "internalType": "uint8", "name": "role", "type": "uint8" }
            ],
            "name": "assignRole",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
        }
    ]
    """;

    public BlockchainService(IOptions<Models.ContractOptions> options, ILogger<BlockchainService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.PharmaNftAddress)
            && !string.IsNullOrWhiteSpace(_options.OwnerPrivateKey)
            && !string.IsNullOrWhiteSpace(_options.RpcUrl);
    }

    private Account CreateAccount()
    {
        return _options.ChainId.HasValue
            ? new Account(_options.OwnerPrivateKey, _options.ChainId.Value)
            : new Account(_options.OwnerPrivateKey);
    }

    public async Task<OnChainTransactionResult> AssignRoleAsync(string targetAddress, string role)
    {
        if (!IsConfigured())
        {
            return new OnChainTransactionResult(false, null, "Blockchain chưa được cấu hình");
        }

        if (!RoleMap.TryGetValue(role, out var roleValue))
        {
            return new OnChainTransactionResult(false, null, $"Role {role} không hợp lệ để đồng bộ on-chain");
        }

        try
        {
            var checksumTarget = new AddressUtil().ConvertToChecksumAddress(targetAddress);
            var account = CreateAccount();
            var web3 = new Web3(account, _options.RpcUrl);
            var contract = web3.Eth.GetContract(AssignRoleAbi, _options.PharmaNftAddress);
            var assignRoleFunction = contract.GetFunction("assignRole");

            var gas = await assignRoleFunction.EstimateGasAsync(account.Address, null, null, checksumTarget, roleValue);
            var receipt = await assignRoleFunction.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                new HexBigInteger(gas.Value),
                null,
                null,
                checksumTarget,
                roleValue);

            if (receipt.Status.Value == 1)
            {
                return new OnChainTransactionResult(true, receipt.TransactionHash, null);
            }

            return new OnChainTransactionResult(false, receipt.TransactionHash, "Giao dịch assignRole thất bại (status != 1)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đồng bộ role {Role} cho địa chỉ {Address} lên on-chain", role, targetAddress);
            return new OnChainTransactionResult(false, null, ex.Message);
        }
    }
}


