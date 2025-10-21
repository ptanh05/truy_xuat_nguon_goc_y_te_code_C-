namespace PharmaDNA.Models
{
    public class CreateApiKeyRequest
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? RateLimit { get; set; }
    }
}
