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
        
        // Navigation properties
        public Comment Comment { get; set; } = null!;
    }
}
