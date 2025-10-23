namespace PharmaDNA.Models
{
    public class CreateResolutionRequest
    {
        public int DisputeId { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public string? ResolvedByUserId { get; set; }
        public string? ResolutionType { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
    }
}
