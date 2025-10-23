namespace PharmaDNA.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty; // create, update, delete
        public string UserAddress { get; set; } = string.Empty;
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? PerformedBy { get; set; }
        public string? PerformedByName { get; set; }
        public string? Changes { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}