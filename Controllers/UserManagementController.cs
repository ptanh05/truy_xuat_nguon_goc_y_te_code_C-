using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        // User Management Endpoints
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                CompanyName = request.CompanyName
            };

            var createdUser = await _userManagementService.CreateUserAsync(user, request.Password);
            return Ok(new { message = "User created successfully", userId = createdUser.Id });
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
        {
            var users = await _userManagementService.GetAllUsersAsync(pageNumber, pageSize);
            return Ok(users);
        }

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequest request)
        {
            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.CompanyName = request.CompanyName;

            await _userManagementService.UpdateUserAsync(user);
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var result = await _userManagementService.DeleteUserAsync(userId);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost("users/{userId}/lock")]
        public async Task<IActionResult> LockUser(int userId)
        {
            var result = await _userManagementService.LockUserAsync(userId);
            if (!result) return NotFound();
            return Ok(new { message = "User locked successfully" });
        }

        [HttpPost("users/{userId}/unlock")]
        public async Task<IActionResult> UnlockUser(int userId)
        {
            var result = await _userManagementService.UnlockUserAsync(userId);
            if (!result) return NotFound();
            return Ok(new { message = "User unlocked successfully" });
        }

        [HttpPost("users/{userId}/reset-password")]
        public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordRequest request)
        {
            var result = await _userManagementService.ResetPasswordAsync(userId, request.NewPassword);
            if (!result) return NotFound();
            return Ok(new { message = "Password reset successfully" });
        }

        // Role Management Endpoints
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var role = new Role
            {
                Name = request.Name,
                Description = request.Description
            };

            var createdRole = await _userManagementService.CreateRoleAsync(role);
            return Ok(new { message = "Role created successfully", roleId = createdRole.Id });
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _userManagementService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpPut("roles/{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, [FromBody] UpdateRoleRequest request)
        {
            var role = await _userManagementService.GetRoleByIdAsync(roleId);
            if (role == null) return NotFound();

            role.Name = request.Name;
            role.Description = request.Description;

            await _userManagementService.UpdateRoleAsync(role);
            return Ok(new { message = "Role updated successfully" });
        }

        // User Role Assignment
        [HttpPost("users/{userId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRoleToUser(int userId, int roleId)
        {
            var result = await _userManagementService.AssignRoleToUserAsync(userId, roleId);
            if (!result) return NotFound();
            return Ok(new { message = "Role assigned successfully" });
        }

        [HttpDelete("users/{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            var result = await _userManagementService.RemoveRoleFromUserAsync(userId, roleId);
            if (!result) return NotFound();
            return Ok(new { message = "Role removed successfully" });
        }

        [HttpGet("users/{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var roles = await _userManagementService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        // Login History
        [HttpGet("users/{userId}/login-history")]
        public async Task<IActionResult> GetLoginHistory(int userId, int days = 30)
        {
            var history = await _userManagementService.GetUserLoginHistoryAsync(userId, days);
            return Ok(history);
        }
    }
}
