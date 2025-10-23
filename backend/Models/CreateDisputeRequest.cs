namespace PharmaDNA.Models
{
    public class CreateDisputeRequest
    {
        public int NFTId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ReportedByUserId { get; set; }
        public string? Priority { get; set; }
    }
}
