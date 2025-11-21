using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Services;

public interface IRoleService
{
    Task<IEnumerable<UserSummary>> GetUsersAsync();
    ConfigStatus GetConfigStatus();
    Task<RoleAssignmentResult> AssignRoleAsync(string address, string role);
    Task DeleteUserAsync(string address);
    /// <summary>
    /// Tự động cấp quyền MANUFACTURER cho địa chỉ ví nếu chưa có quyền
    /// </summary>
    Task<RoleAssignmentResult> AutoAssignRoleAsync(string address);
}

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly ContractOptions _contractOptions;
    private readonly IConfiguration _configuration;
    private readonly BlockchainService _blockchainService;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        ApplicationDbContext context,
        IOptions<ContractOptions> contractOptions,
        IConfiguration configuration,
        BlockchainService blockchainService,
        ILogger<RoleService> logger)
    {
        _context = context;
        _contractOptions = contractOptions.Value;
        _configuration = configuration;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<IEnumerable<UserSummary>> GetUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(u => new UserSummary(u.Address.ToLower(), u.Role, u.AssignedAt));
    }

    public ConfigStatus GetConfigStatus()
    {
        var hasDatabaseUrl = !string.IsNullOrWhiteSpace(_configuration["DATABASE_URL"]) ||
            !string.IsNullOrWhiteSpace(_configuration.GetConnectionString("DefaultConnection"));

        var pinataGateway = _configuration["PINATA_GATEWAY"] ?? "";

        return new ConfigStatus(
            hasDatabaseUrl,
            !string.IsNullOrWhiteSpace(_configuration["PINATA_JWT"]),
            pinataGateway,
            !string.IsNullOrWhiteSpace(_contractOptions.PharmaNftAddress),
            !string.IsNullOrWhiteSpace(_contractOptions.OwnerPrivateKey),
            !string.IsNullOrWhiteSpace(_contractOptions.RpcUrl)
        );
    }

    public async Task<RoleAssignmentResult> AssignRoleAsync(string address, string role)
    {
        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Address and role are required");
        }

        var normalizedAddress = address.ToLower();
        var now = DateTime.UtcNow;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Address == normalizedAddress);
        if (existingUser != null)
        {
            existingUser.Role = role;
            existingUser.AssignedAt = now;
            _context.Users.Update(existingUser);
        }
        else
        {
            await _context.Users.AddAsync(new User
            {
                Address = normalizedAddress,
                Role = role,
                AssignedAt = now
            });
        }

        await _context.SaveChangesAsync();

        var onChainResult = await _blockchainService.AssignRoleAsync(normalizedAddress, role);
        if (!onChainResult.Success)
        {
            await transaction.RollbackAsync();
            var errorMessage = onChainResult.Error ?? "Không rõ lỗi";
            _logger.LogWarning("Đồng bộ role {Role} cho địa chỉ {Address} thất bại: {Error}", role, normalizedAddress, errorMessage);

            var failureMessage = $"⚠️ Không thể đồng bộ quyền {role} cho địa chỉ {normalizedAddress} lên on-chain: {errorMessage}";
            return new RoleAssignmentResult(false, failureMessage, _blockchainService.IsConfigured(), onChainResult.TransactionHash, onChainResult.Error);
        }

        await transaction.CommitAsync();
        _logger.LogInformation("Đã đồng bộ role {Role} cho địa chỉ {Address} lên on-chain. TxHash: {TxHash}", role, normalizedAddress, onChainResult.TransactionHash);

        var successMessage = $"✅ Đã cấp quyền {role} cho địa chỉ {normalizedAddress} và đồng bộ on-chain thành công.";
        return new RoleAssignmentResult(true, successMessage, true, onChainResult.TransactionHash, null);
    }

    public async Task DeleteUserAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address is required");
        }

        var normalized = address.ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Address == normalized);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Tự động cấp quyền MANUFACTURER cho địa chỉ ví nếu chưa có quyền
    /// </summary>
    public async Task<RoleAssignmentResult> AutoAssignRoleAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address is required");
        }

        var normalizedAddress = address.ToLower();
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Address == normalizedAddress);
        
        // Nếu đã có quyền rồi thì không làm gì
        if (existingUser != null && existingUser.Role == "MANUFACTURER")
        {
            return new RoleAssignmentResult(true, $"Địa chỉ {normalizedAddress} đã có quyền MANUFACTURER", _blockchainService.IsConfigured(), null, null);
        }

        // Tự động cấp quyền MANUFACTURER
        return await AssignRoleAsync(normalizedAddress, "MANUFACTURER");
    }
}

public record UserSummary(string Address, string Role, DateTime AssignedAt);

public record ConfigStatus(
    bool DatabaseConfigured,
    bool PinataJwtConfigured,
    string PinataGateway,
    bool PharmaNftConfigured,
    bool OwnerPrivateKeyConfigured,
    bool RpcConfigured);

public record RoleAssignmentResult(bool Success, string Message, bool OnChainConfigured, string? TransactionHash, string? OnChainError);


