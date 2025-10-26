namespace PharmaDNA.Web.Models.DTOs
{
    public class NFTDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ManufacturerAddress { get; set; } = string.Empty;
        public string? DistributorAddress { get; set; }
        public string? PharmacyAddress { get; set; }
        public string? IpfsHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
