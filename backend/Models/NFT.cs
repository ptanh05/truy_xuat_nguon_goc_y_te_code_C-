namespace PharmaDNA.Models
{
    public class NFT
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // pending, active, transferred_to_distributor, transferred_to_pharmacy
        public string ManufacturerAddress { get; set; } = string.Empty;
        public string DistributorAddress { get; set; } = string.Empty;
        public string PharmacyAddress { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string MetadataUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Additional properties needed by services
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string? BatchId { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public string? ProductType { get; set; }
        public DateTime? CreatedDate { get; set; }

        // Navigation properties
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    }
}
