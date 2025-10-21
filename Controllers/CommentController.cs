using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            var result = await _commentService.CreateCommentAsync(comment);
            return Ok(result);
        }

        [HttpGet("{entityType}/{entityId}")]
        public async Task<IActionResult> GetComments(string entityType, int entityId)
        {
            var comments = await _commentService.GetCommentsAsync(entityType, entityId);
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetComment(int id)
        {
            var comment = await _commentService.GetCommentAsync(id);
            return Ok(comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] string content)
        {
            var result = await _commentService.UpdateCommentAsync(id, content);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var result = await _commentService.DeleteCommentAsync(id);
            return Ok(new { success = result });
        }

        [HttpGet("replies/{parentCommentId}")]
        public async Task<IActionResult> GetReplies(int parentCommentId)
        {
            var replies = await _commentService.GetRepliesAsync(parentCommentId);
            return Ok(replies);
        }

        [HttpPost("{commentId}/like")]
        public async Task<IActionResult> LikeComment(int commentId, [FromQuery] string userAddress)
        {
            var result = await _commentService.LikeCommentAsync(commentId, userAddress);
            return Ok(new { success = result });
        }

        [HttpDelete("{commentId}/like")]
        public async Task<IActionResult> UnlikeComment(int commentId, [FromQuery] string userAddress)
        {
            var result = await _commentService.UnlikeCommentAsync(commentId, userAddress);
            return Ok(new { success = result });
        }
    }
}
