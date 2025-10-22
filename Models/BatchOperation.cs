namespace PharmaDNA.Models
{
    public class BatchOperation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // create, update, delete, transfer
        public string Status { get; set; } = string.Empty; // pending, processing, completed, failed
        public string CreatedByAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Navigation properties
        public ICollection<BatchOperationDetail> Details { get; set; } = new List<BatchOperationDetail>();
    }
}