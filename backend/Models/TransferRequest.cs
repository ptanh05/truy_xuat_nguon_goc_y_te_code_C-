namespace PharmaDNA.Models
{
    public class TransferRequest
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string DistributorAddress { get; set; } = string.Empty;
        public string PharmacyAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // pending, approved, rejected
        public string BlockchainTxHash { get; set; } = string.Empty;
        public string BlockchainStatus { get; set; } = string.Empty; // pending, confirmed, failed
        public string ValidationData { get; set; } = string.Empty; // JSON
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Additional properties needed by services
        public DateTime? RequestDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? FromAddress { get; set; }
        public string? ToAddress { get; set; }
        public string? FromUserId { get; set; }
        public string? ToUserId { get; set; }
        public int? Quantity { get; set; }

        // Navigation properties
        public NFT NFT { get; set; } = null!;
    }
}
