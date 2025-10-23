namespace PharmaDNA.Models
{
    public class AddCommentRequest
    {
        public int DisputeId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public bool? IsInternal { get; set; }
    }
}
