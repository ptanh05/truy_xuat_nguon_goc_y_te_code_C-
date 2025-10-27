using Microsoft.EntityFrameworkCore;
using PharmaDNA.Web.Data;
using PharmaDNA.Web.Models.DTOs;

namespace PharmaDNA.Web.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlockchainService _blockchainService;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, IBlockchainService blockchainService, ILogger<UserService> logger)
        {
            _context = context;
            _blockchainService = blockchainService;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return users.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<UserDto>();
            }
        }

        public async Task<UserDto?> GetUserByAddressAsync(string address)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Address.ToLower() == address.ToLower());
                return user != null ? MapToDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by address: {address}");
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(string address, string role)
        {
            try
            {
                var user = new Models.Entities.User
                {
                    Address = address.ToLower(),
                    Role = role,
                    AssignedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created: {address} with role {role}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating user: {address}");
                return false;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(string address, string role)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Address.ToLower() == address.ToLower());

                if (user == null)
                {
                    return await CreateUserAsync(address, role);
                }

                user.Role = role;
                user.AssignedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User role updated: {address} -> {role}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user role: {address}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string address)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Address.ToLower() == address.ToLower());

                if (user == null) return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User deleted: {address}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user: {address}");
                return false;
            }
        }

        public async Task<bool> AssignRoleAsync(string address, string role)
        {
            try
            {
                // Map role string to enum value
                var roleEnumMap = new Dictionary<string, int>
                {
                    { "MANUFACTURER", 1 },
                    { "DISTRIBUTOR", 2 },
                    { "PHARMACY", 3 },
                    { "ADMIN", 4 }
                };

                if (!roleEnumMap.TryGetValue(role.ToUpper(), out var roleEnum))
                {
                    _logger.LogError($"Invalid role: {role}");
                    return false;
                }

                // Lowercase address for consistency
                var normalizedAddress = address.ToLower();

                // Update database first (upsert)
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO users (address, role, assigned_at) VALUES ({0}, {1}, {2}) " +
                    "ON CONFLICT (address) DO UPDATE SET role = {1}, assigned_at = {2}",
                    normalizedAddress, role, DateTime.UtcNow
                );

                // Update blockchain
                var blockchainSuccess = await _blockchainService.AssignRoleAsync(address, roleEnum);
                if (!blockchainSuccess)
                {
                    _logger.LogWarning($"Database updated but failed to assign role on blockchain: {address}");
                    // Don't fail completely, database update succeeded
                }

                // Verify role was set on chain
                var roleOnChain = await _blockchainService.GetRoleAsync(address);
                
                _logger.LogInformation($"Role assigned successfully: {address} -> {role} (DB: {role}, Chain: {roleOnChain})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role: {address} -> {role}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAndRoleAsync(string address)
        {
            try
            {
                var normalizedAddress = address.ToLower();
                
                // Delete from database
                var deleted = await DeleteUserAsync(address);
                if (!deleted) return false;

                // TODO: Revoke role on blockchain
                // await _blockchainService.RevokeRoleAsync(address);

                _logger.LogInformation($"User deleted: {address}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user: {address}");
                return false;
            }
        }

        private UserDto MapToDto(Models.Entities.User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Address = user.Address,
                Role = user.Role,
                AssignedAt = user.AssignedAt
            };
        }
    }
}
