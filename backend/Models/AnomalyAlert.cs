namespace PharmaDNA.Models
{
    public class AnomalyAlert
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // unusual_movement, duplicate_nft, suspicious_activity
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // low, medium, high, critical
        public string Data { get; set; } = string.Empty; // JSON data
        public bool IsResolved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public DateTime? DetectedAt { get; set; }
    }
}
