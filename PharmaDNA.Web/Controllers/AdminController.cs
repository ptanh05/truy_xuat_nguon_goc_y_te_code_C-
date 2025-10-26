using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Web.Models.ViewModels;
using PharmaDNA.Web.Services;

namespace PharmaDNA.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IBlockchainService _blockchainService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserService userService,
            IBlockchainService blockchainService,
            ILogger<AdminController> logger)
        {
            _userService = userService;
            _blockchainService = blockchainService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var model = new UserManagementViewModel
                {
                    Users = users,
                    NewUser = new AdminViewModel()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin page");
                return View(new UserManagementViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(AdminViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Dữ liệu không hợp lệ" });
                }

                var success = await _userService.AssignRoleAsync(model.Address, model.Role);
                
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Đã cấp quyền {model.Role} cho địa chỉ {model.Address} thành công!" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        error = "Không thể cấp quyền. Vui lòng kiểm tra lại thông tin." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role to {model.Address}");
                return Json(new { 
                    success = false, 
                    error = "Có lỗi xảy ra khi cấp quyền" 
                });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                {
                    return Json(new { success = false, error = "Địa chỉ không được để trống" });
                }

                var success = await _userService.DeleteUserAsync(address);
                
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Đã xóa người dùng {address} thành công!" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        error = "Không thể xóa người dùng" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user {address}");
                return Json(new { 
                    success = false, 
                    error = "Có lỗi xảy ra khi xóa người dùng" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AutoAssignRole(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                {
                    return Json(new { success = false, error = "Địa chỉ không được để trống" });
                }

                // Check if user already exists
                var existingUser = await _userService.GetUserByAddressAsync(address);
                if (existingUser != null)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Người dùng {address} đã tồn tại với vai trò {existingUser.Role}" 
                    });
                }

                // Auto-assign based on some logic (e.g., first user gets ADMIN role)
                var allUsers = await _userService.GetAllUsersAsync();
                var role = allUsers.Count == 0 ? "ADMIN" : "MANUFACTURER";

                var success = await _userService.AssignRoleAsync(address, role);
                
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Đã tự động cấp quyền {role} cho {address}" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        error = "Không thể tự động cấp quyền" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error auto-assigning role to {address}");
                return Json(new { 
                    success = false, 
                    error = "Có lỗi xảy ra khi tự động cấp quyền" 
                });
            }
        }
    }
}
