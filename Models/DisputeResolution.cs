namespace PharmaDNA.Models
{
    public class DisputeResolution
    {
        public int Id { get; set; }
        public int DisputeId { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public string ResolvedByAddress { get; set; } = string.Empty;
        public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Dispute Dispute { get; set; } = null!;
    }
}