using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisputeController : ControllerBase
    {
        private readonly IDisputeService _disputeService;

        public DisputeController(IDisputeService disputeService)
        {
            _disputeService = disputeService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDispute([FromBody] CreateDisputeRequest request)
        {
            var dispute = new Dispute
            {
                NFTId = request.NFTId,
                ReportedByUserId = request.ReportedByUserId,
                DisputeType = "General",
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority ?? "Medium"
            };

            var createdDispute = await _disputeService.CreateDisputeAsync(dispute);
            return Ok(new { message = "Dispute created successfully", disputeId = createdDispute.Id });
        }

        [HttpGet("{disputeId}")]
        public async Task<IActionResult> GetDispute(int disputeId)
        {
            var dispute = await _disputeService.GetDisputeAsync(disputeId);
            if (dispute == null) return NotFound();
            return Ok(dispute);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDisputes(int pageNumber = 1, int pageSize = 10)
        {
            var disputes = await _disputeService.GetAllDisputesAsync(pageNumber, pageSize);
            return Ok(disputes);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetDisputesByStatus(string status, int pageNumber = 1, int pageSize = 10)
        {
            var disputes = await _disputeService.GetDisputesByStatusAsync(status, pageNumber, pageSize);
            return Ok(disputes);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDisputesByUser(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var disputes = await _disputeService.GetDisputesByUserAsync(userId, pageNumber, pageSize);
            return Ok(disputes);
        }

        [HttpPut("{disputeId}/status")]
        public async Task<IActionResult> UpdateDisputeStatus(int disputeId, [FromBody] UpdateStatusRequest request)
        {
            var result = await _disputeService.UpdateDisputeStatusAsync(disputeId, request.Status);
            if (!result) return NotFound();
            return Ok(new { message = "Dispute status updated successfully" });
        }

        [HttpPut("{disputeId}/assign")]
        public async Task<IActionResult> AssignDispute(int disputeId, [FromBody] AssignDisputeRequest request)
        {
            var result = await _disputeService.AssignDisputeAsync(disputeId, int.Parse(request.UserId ?? "0"));
            if (!result) return NotFound();
            return Ok(new { message = "Dispute assigned successfully" });
        }

        [HttpPost("{disputeId}/comments")]
        public async Task<IActionResult> AddComment(int disputeId, [FromBody] AddCommentRequest request)
        {
            var result = await _disputeService.AddCommentAsync(disputeId, int.Parse(request.UserId ?? "0"), request.Content, request.IsInternal ?? false);
            if (!result) return BadRequest();
            return Ok(new { message = "Comment added successfully" });
        }

        [HttpGet("{disputeId}/comments")]
        public async Task<IActionResult> GetDisputeComments(int disputeId)
        {
            var comments = await _disputeService.GetDisputeCommentsAsync(disputeId);
            return Ok(comments);
        }

        [HttpPost("{disputeId}/resolutions")]
        public async Task<IActionResult> CreateResolution(int disputeId, [FromBody] CreateResolutionRequest request)
        {
            var resolution = new DisputeResolution
            {
                DisputeId = disputeId,
                ResolvedByUserId = request.ResolvedByUserId,
                ResolutionType = request.ResolutionType,
                Description = request.Description,
                Amount = request.Amount
            };

            var createdResolution = await _disputeService.CreateResolutionAsync(resolution);
            return Ok(new { message = "Resolution created successfully", resolutionId = createdResolution.Id });
        }

        [HttpGet("{disputeId}/resolutions")]
        public async Task<IActionResult> GetDisputeResolutions(int disputeId)
        {
            var resolutions = await _disputeService.GetDisputeResolutionsAsync(disputeId);
            return Ok(resolutions);
        }

        [HttpPut("resolutions/{resolutionId}/approve")]
        public async Task<IActionResult> ApproveResolution(int resolutionId)
        {
            var result = await _disputeService.ApproveResolutionAsync(resolutionId);
            if (!result) return NotFound();
            return Ok(new { message = "Resolution approved successfully" });
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _disputeService.GetDisputeStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("by-type")]
        public async Task<IActionResult> GetDisputesByType()
        {
            var disputes = await _disputeService.GetDisputesByTypeAsync();
            return Ok(disputes);
        }
    }
}
