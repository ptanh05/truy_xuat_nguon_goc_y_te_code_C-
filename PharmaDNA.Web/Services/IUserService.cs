using PharmaDNA.Web.Models.DTOs;

namespace PharmaDNA.Web.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByAddressAsync(string address);
        Task<bool> CreateUserAsync(string address, string role);
        Task<bool> UpdateUserRoleAsync(string address, string role);
        Task<bool> DeleteUserAsync(string address);
        Task<bool> AssignRoleAsync(string address, string role);
    }
}
