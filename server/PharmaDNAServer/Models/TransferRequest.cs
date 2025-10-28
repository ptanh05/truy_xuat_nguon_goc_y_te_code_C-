namespace PharmaDNAServer.Models;

public class TransferRequest
{
    public int Id { get; set; }
    public int NftId { get; set; }
    public string DistributorAddress { get; set; } = string.Empty;
    public string? PharmacyAddress { get; set; }
    public string? TransferNote { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

