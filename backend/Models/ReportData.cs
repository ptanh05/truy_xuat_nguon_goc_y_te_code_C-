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
        public string? ReportName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? GeneratedBy { get; set; }
        public string? Content { get; set; }
        public bool? IsPublished { get; set; }
        public DateTime? GeneratedDate { get; set; }
    }
}