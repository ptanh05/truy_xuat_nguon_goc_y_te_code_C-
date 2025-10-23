namespace PharmaDNA.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public string AuthorAddress { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public int? ParentCommentId { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public DateTime? UpdatedDate { get; set; }
        public bool? IsEdited { get; set; }
        public int LikeCount { get; set; }
        public DateTime? CreatedDate { get; set; }
        
        // Navigation properties
        public NFT NFT { get; set; } = null!;
        public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}