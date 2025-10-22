namespace PharmaDNA.Models
{
    public class ReportData
    {
        public int Id { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty; // JSON data
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedByAddress { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}