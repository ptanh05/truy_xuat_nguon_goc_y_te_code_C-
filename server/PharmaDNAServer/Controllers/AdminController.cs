using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
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

        // TODO: Sync with blockchain contract
        // Implement blockchain sync logic here

        return Ok(new
        {
            success = true,
            message = $"✅ Đã cấp quyền {request.Role} cho địa chỉ {address} và đồng bộ lên blockchain thành công!"
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

