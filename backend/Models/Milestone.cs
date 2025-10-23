namespace PharmaDNA.Models
{
    public class Milestone
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string Type { get; set; } = string.Empty; // Manufacturing, Distribution, Pharmacy, etc.
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ActorAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public NFT NFT { get; set; } = null!;
    }
}
