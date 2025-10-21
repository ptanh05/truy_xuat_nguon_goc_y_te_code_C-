using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class CommentService : ICommentService
    {
        private readonly PharmaDNAContext _context;
        private readonly ILogger<CommentService> _logger;

        public CommentService(PharmaDNAContext context, ILogger<CommentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            try
            {
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Comment created for {comment.EntityType} {comment.EntityId}");
                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating comment: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Comment>> GetCommentsAsync(string entityType, int entityId)
        {
            return await _context.Comments
                .Where(c => c.EntityType == entityType && c.EntityId == entityId && !c.IsDeleted && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .Include(c => c.Likes)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Comment> GetCommentAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Replies)
                .Include(c => c.Likes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comment> UpdateCommentAsync(int id, string newContent)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return null;

            comment.Content = newContent;
            comment.UpdatedDate = DateTime.UtcNow;
            comment.IsEdited = true;

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;

            comment.IsDeleted = true;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Comment>> GetRepliesAsync(int parentCommentId)
        {
            return await _context.Comments
                .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
                .Include(c => c.Likes)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> LikeCommentAsync(int commentId, string userAddress)
        {
            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserAddress == userAddress);

            if (existingLike != null) return false;

            var like = new CommentLike
            {
                CommentId = commentId,
                UserAddress = userAddress
            };

            _context.CommentLikes.Add(like);

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                comment.LikeCount++;
                _context.Comments.Update(comment);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeCommentAsync(int commentId, string userAddress)
        {
            var like = await _context.CommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserAddress == userAddress);

            if (like == null) return false;

            _context.CommentLikes.Remove(like);

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null && comment.LikeCount > 0)
            {
                comment.LikeCount--;
                _context.Comments.Update(comment);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCommentLikedAsync(int commentId, string userAddress)
        {
            return await _context.CommentLikes
                .AnyAsync(l => l.CommentId == commentId && l.UserAddress == userAddress);
        }

        public async Task<Note> CreateNoteAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<List<Note>> GetNotesAsync(string entityType, int entityId)
        {
            return await _context.Notes
                .Where(n => n.EntityType == entityType && n.EntityId == entityId && !n.IsArchived)
                .OrderByDescending(n => n.Priority)
                .ThenBy(n => n.DueDate)
                .ToListAsync();
        }

        public async Task<Note> GetNoteAsync(int id)
        {
            return await _context.Notes.FindAsync(id);
        }

        public async Task<Note> UpdateNoteAsync(int id, Note note)
        {
            var existingNote = await _context.Notes.FindAsync(id);
            if (existingNote == null) return null;

            existingNote.Title = note.Title;
            existingNote.Content = note.Content;
            existingNote.Category = note.Category;
            existingNote.Priority = note.Priority;
            existingNote.DueDate = note.DueDate;
            existingNote.UpdatedDate = DateTime.UtcNow;

            _context.Notes.Update(existingNote);
            await _context.SaveChangesAsync();
            return existingNote;
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return false;

            note.IsArchived = true;
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return false;

            note.IsCompleted = true;
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Note>> GetPendingNotesAsync()
        {
            return await _context.Notes
                .Where(n => !n.IsCompleted && !n.IsArchived)
                .OrderByDescending(n => n.Priority)
                .ThenBy(n => n.DueDate)
                .ToListAsync();
        }

        public async Task<List<Note>> GetOverdueNotesAsync()
        {
            return await _context.Notes
                .Where(n => n.DueDate < DateTime.UtcNow && !n.IsCompleted && !n.IsArchived)
                .OrderBy(n => n.DueDate)
                .ToListAsync();
        }

        public async Task<Attachment> AddAttachmentAsync(Attachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<List<Attachment>> GetAttachmentsAsync(int commentId)
        {
            return await _context.Attachments
                .Where(a => a.CommentId == commentId)
                .ToListAsync();
        }

        public async Task<bool> DeleteAttachmentAsync(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null) return false;

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
