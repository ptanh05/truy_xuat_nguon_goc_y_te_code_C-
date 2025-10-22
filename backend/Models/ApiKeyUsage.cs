namespace PharmaDNA.Models
{
    public class ApiKeyUsage
    {
        public int Id { get; set; }
        public int ApiKeyId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int ResponseCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Navigation properties
        public ApiKey ApiKey { get; set; } = null!;
    }
}