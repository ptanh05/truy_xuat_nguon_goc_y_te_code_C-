namespace PharmaDNA.Models
{
    public class InventoryAlert
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Type { get; set; } = string.Empty; // low_stock, expiry, anomaly
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // low, medium, high, critical
        public bool IsResolved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public int? InventoryItemId { get; set; }
        public string? AlertType { get; set; }
        public DateTime? ResolvedDate { get; set; }
        
        // Navigation properties
        public InventoryItem Item { get; set; } = null!;
    }
}