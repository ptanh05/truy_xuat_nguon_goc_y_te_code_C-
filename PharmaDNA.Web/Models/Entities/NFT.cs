using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Web.Models.Entities
{
    public class NFT
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string BatchNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(42)]
        public string ManufacturerAddress { get; set; } = string.Empty;
        
        [MaxLength(42)]
        public string? DistributorAddress { get; set; }
        
        [MaxLength(42)]
        public string? PharmacyAddress { get; set; }
        
        [MaxLength(255)]
        public string? IpfsHash { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ManufactureDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public string? Description { get; set; }
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        // Navigation properties
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
        public ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
    }
}
