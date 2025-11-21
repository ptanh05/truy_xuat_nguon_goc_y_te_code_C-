using Microsoft.EntityFrameworkCore;
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
}

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly ContractOptions _contractOptions;
    private readonly IConfiguration _configuration;

    public RoleService(
        ApplicationDbContext context,
        IOptions<ContractOptions> contractOptions,
        IConfiguration configuration)
    {
        _context = context;
        _contractOptions = contractOptions.Value;
        _configuration = configuration;
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

        var pinataGateway = _configuration["PINATA_GATEWAY"];

        return new ConfigStatus(
            hasDatabaseUrl,
            !string.IsNullOrWhiteSpace(_configuration["PINATA_JWT"]),
            string.IsNullOrWhiteSpace(pinataGateway) ? "https://gateway.pinata.cloud/ipfs/" : pinataGateway!,
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

        var onChainConfigured = !string.IsNullOrWhiteSpace(_contractOptions.PharmaNftAddress) &&
            !string.IsNullOrWhiteSpace(_contractOptions.OwnerPrivateKey);

        var message = onChainConfigured
            ? $"✅ Đã cấp quyền {role} cho địa chỉ {normalizedAddress}."
            : $"✅ Đã cấp quyền {role} cho địa chỉ {normalizedAddress}. ⚠️ Chưa cấu hình PHARMA_NFT_ADDRESS/OWNER_PRIVATE_KEY để đồng bộ on-chain.";

        return new RoleAssignmentResult(true, message, onChainConfigured);
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
}

public record UserSummary(string Address, string Role, DateTime AssignedAt);

public record ConfigStatus(
    bool DatabaseConfigured,
    bool PinataJwtConfigured,
    string PinataGateway,
    bool PharmaNftConfigured,
    bool OwnerPrivateKeyConfigured,
    bool RpcConfigured);

public record RoleAssignmentResult(bool Success, string Message, bool OnChainConfigured);


