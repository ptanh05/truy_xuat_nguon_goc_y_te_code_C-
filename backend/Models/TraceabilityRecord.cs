using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Models
{
    public class TraceabilityRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int NFTId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string BatchId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Manufacturer { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? BlockchainHash { get; set; }
        
        // Navigation properties
        public virtual NFT? NFT { get; set; }
    }
}
