using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaDNAServer.Models;

[Table("SanPhamNFT")]
public class NFT
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? CertificateUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? IpfsHash { get; set; }
    public string? ManufacturerAddress { get; set; }
    public string? DistributorAddress { get; set; }
    public string? PharmacyAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

