using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Web.Models.ViewModels
{
    public class AdminViewModel
    {
        [Required(ErrorMessage = "Địa chỉ ví là bắt buộc")]
        [Display(Name = "Địa chỉ ví")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = string.Empty;
    }

    public class UserManagementViewModel
    {
        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public AdminViewModel NewUser { get; set; } = new AdminViewModel();
    }
}
