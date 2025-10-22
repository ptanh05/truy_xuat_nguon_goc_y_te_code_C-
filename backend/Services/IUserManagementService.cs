using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IUserManagementService
    {
        // User Management
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<User> GetUserByIdAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10);
        Task<bool> LockUserAsync(int userId);
        Task<bool> UnlockUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);

        // Role Management
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int roleId);

        // Permission Management
        Task<Permission> CreatePermissionAsync(Permission permission);
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId);
        Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);

        // User Role Assignment
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
        Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string module, string action);

        // Login History
        Task<bool> LogLoginAttemptAsync(int userId, string ipAddress, string userAgent, bool success, string failureReason = null);
        Task<IEnumerable<UserLoginHistory>> GetUserLoginHistoryAsync(int userId, int days = 30);
    }
}
