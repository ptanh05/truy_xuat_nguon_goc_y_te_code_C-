using System;

namespace PharmaDNA.Models
{
    public class ApiKeyUsage
    {
        public int Id { get; set; }
        public int ApiKeyId { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public DateTime UsedAt { get; set; }
        public string IpAddress { get; set; }

        // Navigation properties
        public ApiKey ApiKey { get; set; }
    }
}
