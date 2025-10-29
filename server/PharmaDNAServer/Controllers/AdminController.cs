using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using Microsoft.Extensions.Options;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PharmaDNAServer.Models.ContractOptions _contractOptions;
    private readonly IConfiguration _configuration;

    public AdminController(ApplicationDbContext context, IOptions<PharmaDNAServer.Models.ContractOptions> contractOptions, IConfiguration configuration)
    {
        _context = context;
        _contractOptions = contractOptions.Value;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        var response = users.Select(u => new
        {
            address = u.Address.ToLower(),
            role = u.Role,
            assignedAt = u.AssignedAt
        });
        return Ok(response);
    }

    [HttpGet("config-status")]
    public IActionResult GetConfigStatus()
    {
        var hasDatabaseUrl = !string.IsNullOrWhiteSpace(_configuration["DATABASE_URL"]) || !string.IsNullOrWhiteSpace(_configuration.GetConnectionString("DefaultConnection"));
        var hasPinataJwt = !string.IsNullOrWhiteSpace(_configuration["PINATA_JWT"]);
        var hasPharmaNft = !string.IsNullOrWhiteSpace(_contractOptions.PharmaNftAddress);
        var hasOwnerPk = !string.IsNullOrWhiteSpace(_contractOptions.OwnerPrivateKey);
        var hasRpc = !string.IsNullOrWhiteSpace(_contractOptions.RpcUrl);
        var pinataGateway = _configuration["PINATA_GATEWAY"];

        return Ok(new
        {
            databaseConfigured = hasDatabaseUrl,
            pinataJwtConfigured = hasPinataJwt,
            pinataGateway = string.IsNullOrWhiteSpace(pinataGateway) ? "https://gateway.pinata.cloud/ipfs/" : pinataGateway,
            pharmaNftConfigured = hasPharmaNft,
            ownerPrivateKeyConfigured = hasOwnerPk,
            rpcConfigured = hasRpc
        });
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.Address) || string.IsNullOrEmpty(request.Role))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var address = request.Address.ToLower();
        var now = DateTime.UtcNow;

        // Check if user exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Address == address);
        
        if (existingUser != null)
        {
            existingUser.Role = request.Role;
            existingUser.AssignedAt = now;
            _context.Users.Update(existingUser);
        }
        else
        {
            var newUser = new User
            {
                Address = address,
                Role = request.Role,
                AssignedAt = now
            };
            await _context.Users.AddAsync(newUser);
        }

        await _context.SaveChangesAsync();

        // Validate blockchain config presence (prep for signing)
        if (string.IsNullOrWhiteSpace(_contractOptions.PharmaNftAddress) ||
            string.IsNullOrWhiteSpace(_contractOptions.OwnerPrivateKey))
        {
            // Return success for DB but include warning about blockchain config
            return Ok(new
            {
                success = true,
                message = $"✅ Đã cấp quyền {request.Role} cho địa chỉ {address}. ⚠️ Chưa cấu hình PHARMA_NFT_ADDRESS/OWNER_PRIVATE_KEY để đồng bộ on-chain.",
                onChainConfigured = false
            });
        }

        return Ok(new
        {
            success = true,
            message = $"✅ Đã cấp quyền {request.Role} cho địa chỉ {address}.",
            onChainConfigured = true
        });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Address))
        {
            return BadRequest(new { error = "Thiếu địa chỉ" });
        }

        var address = request.Address.ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Address == address);
        
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return Ok(new { success = true });
    }
}

public class AssignRoleRequest
{
    public string Address { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class DeleteUserRequest
{
    public string Address { get; set; } = string.Empty;
}

