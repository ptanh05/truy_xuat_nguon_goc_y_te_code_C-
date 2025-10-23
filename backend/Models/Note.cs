namespace PharmaDNA.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public bool? IsArchived { get; set; }
        public bool? IsCompleted { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        // Navigation properties
        public Comment Comment { get; set; } = null!;
    }
}
