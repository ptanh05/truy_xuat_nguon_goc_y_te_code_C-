using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class ApiKey
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int RateLimit { get; set; } // requests per minute
        public string AllowedIPs { get; set; } // comma-separated

        // Navigation properties
        public User User { get; set; }
        public ICollection<ApiKeyUsage> Usages { get; set; } = new List<ApiKeyUsage>();
    }
}
