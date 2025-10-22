namespace PharmaDNA.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public NFT NFT { get; set; } = null!;
        public InventoryLocation Location { get; set; } = null!;
        public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
    }
}