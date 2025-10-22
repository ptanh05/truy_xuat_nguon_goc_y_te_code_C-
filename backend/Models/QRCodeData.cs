namespace PharmaDNA.Models
{
    public class QRCodeData
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty; // JSON data
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? QRCodeContent { get; set; }
        public string? QRCodeImageUrl { get; set; }
        public string? BarcodeContent { get; set; }
        public string? BarcodeImageUrl { get; set; }
        public string? Format { get; set; }
        public int? ScanCount { get; set; }
        public DateTime? LastScannedDate { get; set; }
        
        // Navigation properties
        public NFT NFT { get; set; } = null!;
        public ICollection<QRScanLog> ScanLogs { get; set; } = new List<QRScanLog>();
    }
}