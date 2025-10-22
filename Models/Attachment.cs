namespace PharmaDNA.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string UploadedByAddress { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Comment Comment { get; set; } = null!;
    }
}
