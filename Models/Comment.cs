using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string EntityType { get; set; } // "NFT", "TransferRequest", "InventoryItem"
        public int EntityId { get; set; }
        public string Content { get; set; }
        public string AuthorAddress { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int? ParentCommentId { get; set; } // For nested replies
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public int LikeCount { get; set; }

        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }

    public class CommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string UserAddress { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual Comment Comment { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
        public string EntityType { get; set; } // "NFT", "TransferRequest", "InventoryItem"
        public int EntityId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; } // "General", "Warning", "Important", "Follow-up"
        public string Color { get; set; } // For visual categorization
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int Priority { get; set; } // 1-5, 5 being highest
        public bool IsArchived { get; set; }
    }

    public class Attachment
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        public virtual Comment Comment { get; set; }
    }
}
