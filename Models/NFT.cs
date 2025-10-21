namespace PharmaDNA.Models
{
    public class NFT
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BatchNumber { get; set; }
        public string Status { get; set; } // pending, active, transferred_to_distributor, transferred_to_pharmacy
        public string ManufacturerAddress { get; set; }
        public string DistributorAddress { get; set; }
        public string PharmacyAddress { get; set; }
        public string ImageUrl { get; set; }
        public string MetadataUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    }
}
