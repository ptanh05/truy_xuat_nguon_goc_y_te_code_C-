using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Data;
using PharmaDNA.Models;
using PharmaDNA.Attributes;
using Microsoft.EntityFrameworkCore;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireRole("Admin")]
    public class AdminController : ControllerBase
    {
        private readonly PharmaDNAContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(PharmaDNAContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var totalNFTs = await _context.NFTs.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var totalTransfers = await _context.TransferRequests.CountAsync();
                var pendingTransfers = await _context.TransferRequests.CountAsync(r => r.Status == "pending");

                var nftsByStatus = await _context.NFTs
                    .GroupBy(n => n.Status)
                    .Select(g => new { status = g.Key, count = g.Count() })
                    .ToListAsync();

                var usersByRole = await _context.Users
                    .GroupBy(u => u.Role)
                    .Select(g => new { role = g.Key, count = g.Count() })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalNFTs,
                        totalUsers,
                        totalTransfers,
                        pendingTransfers,
                        nftsByStatus,
                        usersByRole
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting dashboard: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.WalletAddress == request.WalletAddress);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "User already exists" });

                var user = new User
                {
                    WalletAddress = request.WalletAddress,
                    Role = request.Role,
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Location = request.Location,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string role = null)
        {
            try
            {
                var query = _context.Users.AsQueryable();
                if (!string.IsNullOrEmpty(role))
                    query = query.Where(u => u.Role == role);

                var users = await query.ToListAsync();
                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting users: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                user.Role = request.Role ?? user.Role;
                user.Name = request.Name ?? user.Name;
                user.Email = request.Email ?? user.Email;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
                user.Location = request.Location ?? user.Location;
                user.IsActive = request.IsActive ?? user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("nfts")]
        public async Task<IActionResult> GetAllNFTs()
        {
            try
            {
                var nfts = await _context.NFTs.Include(n => n.Milestones).ToListAsync();
                return Ok(new { success = true, data = nfts });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting NFTs: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("transfer-requests")]
        public async Task<IActionResult> GetAllTransferRequests()
        {
            try
            {
                var requests = await _context.TransferRequests.Include(r => r.NFT).ToListAsync();
                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transfer requests: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class CreateUserDto
    {
        public string WalletAddress { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
    }

    public class UpdateUserDto
    {
        public string Role { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public bool? IsActive { get; set; }
    }
}
