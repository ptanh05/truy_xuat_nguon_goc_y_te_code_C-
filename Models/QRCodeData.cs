using System;

namespace PharmaDNA.Models
{
    public class QRCodeData
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string QRCodeContent { get; set; }
        public string QRCodeImageUrl { get; set; }
        public string BarcodeContent { get; set; }
        public string BarcodeImageUrl { get; set; }
        public string Format { get; set; } // "QR", "Code128", "EAN13"
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int ScanCount { get; set; }
        public DateTime LastScannedDate { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual NFT NFT { get; set; }
    }

    public class QRScanLog
    {
        public int Id { get; set; }
        public int QRCodeDataId { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedFrom { get; set; } // IP address or location
        public DateTime ScanDate { get; set; } = DateTime.UtcNow;
        public string DeviceInfo { get; set; }

        public virtual QRCodeData QRCodeData { get; set; }
    }
}
