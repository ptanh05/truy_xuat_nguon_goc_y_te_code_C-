namespace PharmaDNA.Web.Models.DTOs
{
    public class TransferRequestDto
    {
        public int Id { get; set; }
        public int NftId { get; set; }
        public string DistributorAddress { get; set; } = string.Empty;
        public string PharmacyAddress { get; set; } = string.Empty;
        public string? TransferNote { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
