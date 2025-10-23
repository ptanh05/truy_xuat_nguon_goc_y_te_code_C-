namespace PharmaDNA.Models
{
    public class CreateApiKeyRequest
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? RateLimit { get; set; }
    }
}
