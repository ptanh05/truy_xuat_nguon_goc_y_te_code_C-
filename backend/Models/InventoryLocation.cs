namespace PharmaDNA.Models
{
    public class InventoryLocation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // warehouse, pharmacy, distributor
        public string OwnerAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? Quantity { get; set; }
        public string? LocationName { get; set; }
        
        // Navigation properties
        public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }
}