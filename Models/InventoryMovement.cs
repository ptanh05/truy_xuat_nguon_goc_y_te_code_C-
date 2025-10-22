namespace PharmaDNA.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Type { get; set; } = string.Empty; // in, out, transfer
        public int Quantity { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string UserAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public InventoryItem Item { get; set; } = null!;
    }
}