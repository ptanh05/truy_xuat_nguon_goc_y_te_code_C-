using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Web.Models.Entities
{
    public class TransferRequest
    {
        public int Id { get; set; }
        
        public int NftId { get; set; }
        
        [Required]
        [MaxLength(42)]
        public string DistributorAddress { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(42)]
        public string PharmacyAddress { get; set; } = string.Empty;
        
        public string? TransferNote { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public NFT NFT { get; set; } = null!;
    }
}
