namespace PharmaDNA.Models
{
    public class QRScanLog
    {
        public int Id { get; set; }
        public int QRCodeDataId { get; set; }
        public string ScannerAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ScannedBy { get; set; }
        public string? ScannedFrom { get; set; }
        public DateTime? ScanDate { get; set; }
        
        // Navigation properties
        public QRCodeData QRCodeData { get; set; } = null!;
    }
}
