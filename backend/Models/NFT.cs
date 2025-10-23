using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Models
{
    public class NFT
    {
        public int Id { get; set; }
        
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
        
        public DateTime ExpiryDate { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [StringLength(100)]
        public string ProductType { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Pharma Network specific fields
        [StringLength(100)]
        public string? BlockchainTransactionHash { get; set; }
        
        [StringLength(100)]
        public string? BlockchainAddress { get; set; }
        
        [StringLength(100)]
        public string? PharmaNetworkId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
    }
}