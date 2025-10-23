namespace PharmaDNA.Models
{
    public class ApiKey
    {
        public int Id { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public string OwnerAddress { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? Key { get; set; }
        public string? Secret { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? RateLimit { get; set; }
        public User? User { get; set; }
        public string[] Permissions { get; set; } = Array.Empty<string>();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        
        // Navigation properties
        public ICollection<ApiKeyUsage> Usages { get; set; } = new List<ApiKeyUsage>();
    }
}