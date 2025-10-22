namespace PharmaDNA.Models
{
    public class AlertRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // inventory, expiry, anomaly
        public string Condition { get; set; } = string.Empty; // JSON condition
        public string Action { get; set; } = string.Empty; // email, sms, webhook
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
