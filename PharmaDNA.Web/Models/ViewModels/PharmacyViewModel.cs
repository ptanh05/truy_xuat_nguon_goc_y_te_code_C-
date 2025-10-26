using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Web.Models.ViewModels
{
    public class PharmacyViewModel
    {
        [Display(Name = "Số lô thuốc")]
        public string BatchNumber { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ nhà thuốc")]
        public string PharmacyAddress { get; set; } = string.Empty;
    }

    public class DrugLookupResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ManufacturerAddress { get; set; } = string.Empty;
        public string? DistributorAddress { get; set; }
        public string? PharmacyAddress { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<MilestoneInfo> Milestones { get; set; } = new List<MilestoneInfo>();
    }

    public class MilestoneInfo
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActorAddress { get; set; } = string.Empty;
    }
}
