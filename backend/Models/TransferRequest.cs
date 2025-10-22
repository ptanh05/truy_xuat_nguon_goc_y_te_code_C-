namespace PharmaDNA.Models
{
    public class TransferRequest
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string BatchNumber { get; set; }
        public string DistributorAddress { get; set; }
        public string PharmacyAddress { get; set; }
        public string Status { get; set; } // pending, approved, rejected
        public string BlockchainTxHash { get; set; }
        public string BlockchainStatus { get; set; } // pending, confirmed, failed
        public string ValidationData { get; set; } // JSON
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
        public NFT NFT { get; set; }
    }
}
