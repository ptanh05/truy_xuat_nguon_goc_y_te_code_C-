namespace PharmaDNA.Models
{
    public class DisputeComment
    {
        public int Id { get; set; }
        public int DisputeId { get; set; }
        public string AuthorAddress { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public string? Comment { get; set; }
        public bool? IsInternal { get; set; }
        public User? User { get; set; }
        
        // Navigation properties
        public Dispute Dispute { get; set; } = null!;
    }
}