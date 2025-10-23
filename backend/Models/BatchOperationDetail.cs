namespace PharmaDNA.Models
{
    public class BatchOperationDetail
    {
        public int Id { get; set; }
        public int BatchOperationId { get; set; }
        public int NFTId { get; set; }
        public string Status { get; set; } = string.Empty; // pending, success, failed
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
        public int? RecordNumber { get; set; }
        public string? Data { get; set; }
        
        // Navigation properties
        public BatchOperation BatchOperation { get; set; } = null!;
        public NFT NFT { get; set; } = null!;
    }
}