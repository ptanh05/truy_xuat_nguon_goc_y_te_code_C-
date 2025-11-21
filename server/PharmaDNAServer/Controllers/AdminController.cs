using Microsoft.AspNetCore.Mvc;
using PharmaDNAServer.Models;
using PharmaDNAServer.Services;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IRoleService _roleService;

    public AdminController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _roleService.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("config-status")]
    public IActionResult GetConfigStatus()
    {
        var status = _roleService.GetConfigStatus();
        return Ok(status);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.Address) || string.IsNullOrEmpty(request.Role))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var result = await _roleService.AssignRoleAsync(request.Address, request.Role);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Address))
        {
            return BadRequest(new { error = "Thiếu địa chỉ" });
        }

        await _roleService.DeleteUserAsync(request.Address);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Tự động cấp quyền MANUFACTURER cho địa chỉ ví nếu chưa có quyền
    /// </summary>
    [HttpPost("auto-assign-role")]
    public async Task<IActionResult> AutoAssignRole([FromBody] AutoAssignRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.Address))
        {
            return BadRequest(new { error = "Thiếu địa chỉ" });
        }

        try
        {
            var result = await _roleService.AutoAssignRoleAsync(request.Address);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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

public class AutoAssignRoleRequest
{
    public string Address { get; set; } = string.Empty;
}

