namespace PharmaDNA.Models
{
    public class Milestone
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string Type { get; set; } // Manufacturing, Distribution, Pharmacy, etc.
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActorAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public NFT NFT { get; set; }
    }
}
