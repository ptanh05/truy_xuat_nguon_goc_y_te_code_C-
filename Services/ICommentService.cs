using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface ICommentService
    {
        // Comments
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsAsync(string entityType, int entityId);
        Task<Comment> GetCommentAsync(int id);
        Task<Comment> UpdateCommentAsync(int id, string newContent);
        Task<bool> DeleteCommentAsync(int id);
        Task<List<Comment>> GetRepliesAsync(int parentCommentId);

        // Likes
        Task<bool> LikeCommentAsync(int commentId, string userAddress);
        Task<bool> UnlikeCommentAsync(int commentId, string userAddress);
        Task<bool> IsCommentLikedAsync(int commentId, string userAddress);

        // Notes
        Task<Note> CreateNoteAsync(Note note);
        Task<List<Note>> GetNotesAsync(string entityType, int entityId);
        Task<Note> GetNoteAsync(int id);
        Task<Note> UpdateNoteAsync(int id, Note note);
        Task<bool> DeleteNoteAsync(int id);
        Task<bool> CompleteNoteAsync(int id);
        Task<List<Note>> GetPendingNotesAsync();
        Task<List<Note>> GetOverdueNotesAsync();

        // Attachments
        Task<Attachment> AddAttachmentAsync(Attachment attachment);
        Task<List<Attachment>> GetAttachmentsAsync(int commentId);
        Task<bool> DeleteAttachmentAsync(int id);
    }
}
