using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaDNAServer.Models;

/// <summary>
/// Model lưu trữ dữ liệu cảm biến AIoT từ nhà phân phối
/// </summary>
[Table("SensorLogs")]
public class SensorLog
{
    public int Id { get; set; }
    public int NftId { get; set; }
    public string DistributorAddress { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public string? MimeType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ProcessedStatus { get; set; } // pending, processed, failed
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingError { get; set; }

    // Navigation property
    [ForeignKey("NftId")]
    public NFT? NFT { get; set; }
}

