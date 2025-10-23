namespace PharmaDNA.Models
{
    public class Dispute
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // open, in_progress, resolved, closed
        public string Priority { get; set; } = string.Empty; // low, medium, high, critical
        public string ReporterAddress { get; set; } = string.Empty;
        public string AssignedToAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public string? DisputeType { get; set; }
        public decimal? CompensationAmount { get; set; }
        public string? ReportedByUser { get; set; }
        public string? ReportedByUserId { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedToUser { get; set; }
        
        // Navigation properties
        public NFT NFT { get; set; } = null!;
        public ICollection<DisputeComment> Comments { get; set; } = new List<DisputeComment>();
        public ICollection<DisputeResolution> Resolutions { get; set; } = new List<DisputeResolution>();
    }
}