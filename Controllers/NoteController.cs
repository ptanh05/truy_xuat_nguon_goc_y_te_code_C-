using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public NoteController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            var result = await _commentService.CreateNoteAsync(note);
            return Ok(result);
        }

        [HttpGet("{entityType}/{entityId}")]
        public async Task<IActionResult> GetNotes(string entityType, int entityId)
        {
            var notes = await _commentService.GetNotesAsync(entityType, entityId);
            return Ok(notes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNote(int id)
        {
            var note = await _commentService.GetNoteAsync(id);
            return Ok(note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] Note note)
        {
            var result = await _commentService.UpdateNoteAsync(id, note);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var result = await _commentService.DeleteNoteAsync(id);
            return Ok(new { success = result });
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteNote(int id)
        {
            var result = await _commentService.CompleteNoteAsync(id);
            return Ok(new { success = result });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingNotes()
        {
            var notes = await _commentService.GetPendingNotesAsync();
            return Ok(notes);
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueNotes()
        {
            var notes = await _commentService.GetOverdueNotesAsync();
            return Ok(notes);
        }
    }
}
