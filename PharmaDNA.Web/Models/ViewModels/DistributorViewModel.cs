using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Web.Models.ViewModels
{
    public class DistributorViewModel
    {
        [Display(Name = "Chọn lô thuốc")]
        public int? SelectedNFTId { get; set; }

        [Display(Name = "File dữ liệu cảm biến")]
        public IFormFile? SensorFile { get; set; }

        [Display(Name = "Địa chỉ nhà phân phối")]
        public string DistributorAddress { get; set; } = string.Empty;
    }

    public class MilestoneViewModel
    {
        [Required(ErrorMessage = "Loại mốc là bắt buộc")]
        [Display(Name = "Loại mốc")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Vị trí")]
        public string? Location { get; set; }

        [Required]
        public int NftId { get; set; }

        [Required]
        public string ActorAddress { get; set; } = string.Empty;
    }
}
