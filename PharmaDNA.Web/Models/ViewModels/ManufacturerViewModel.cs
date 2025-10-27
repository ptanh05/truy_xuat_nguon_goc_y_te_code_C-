using System.ComponentModel.DataAnnotations;
using PharmaDNA.Web.Attributes;

namespace PharmaDNA.Web.Models.ViewModels
{
    public class ManufacturerViewModel
    {
        [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên thuốc không được quá 255 ký tự")]
        [Display(Name = "Tên thuốc")]
        public string DrugName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lô là bắt buộc")]
        [ValidBatchNumber]
        [Display(Name = "Số lô")]
        public string BatchNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sản xuất là bắt buộc")]
        [Display(Name = "Ngày sản xuất")]
        [DataType(DataType.Date)]
        public DateTime ManufacturingDate { get; set; }

        [Required(ErrorMessage = "Hạn dùng là bắt buộc")]
        [Display(Name = "Hạn dùng")]
        [DataType(DataType.Date)]
        [ValidExpiryDate(nameof(ManufacturingDate))]
        public DateTime ExpiryDate { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Địa chỉ nhà sản xuất là bắt buộc")]
        [ValidEthereumAddress]
        [Display(Name = "Địa chỉ nhà sản xuất")]
        public string ManufacturerAddress { get; set; } = string.Empty;

        [Display(Name = "Ảnh thuốc")]
        public IFormFile? DrugImage { get; set; }

        [Display(Name = "Chứng chỉ")]
        public IFormFile? Certificate { get; set; }
    }
}
