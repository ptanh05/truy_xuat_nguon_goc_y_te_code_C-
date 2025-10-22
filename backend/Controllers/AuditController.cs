using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] string entityType = null, 
            [FromQuery] int? entityId = null, [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            var logs = await _auditService.GetAuditLogsAsync(entityType, entityId, startDate, endDate);
            return Ok(logs);
        }

        [HttpGet("user/{performedBy}")]
        public async Task<IActionResult> GetUserActivity(string performedBy, 
            [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var activity = await _auditService.GetUserActivityAsync(performedBy, startDate, endDate);
            return Ok(activity);
        }

        [HttpGet("entity/{entityType}/{entityId}/history")]
        public async Task<IActionResult> GetEntityHistory(string entityType, int entityId)
        {
            var history = await _auditService.GetEntityHistoryAsync(entityType, entityId);
            return Ok(history);
        }

        [HttpGet("entity/{entityType}/{entityId}/version/{version}")]
        public async Task<IActionResult> GetEntityVersion(string entityType, int entityId, int version)
        {
            var entityVersion = await _auditService.GetEntityVersionAsync(entityType, entityId, version);
            return Ok(entityVersion);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetActivitySummary([FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var summary = await _auditService.GetActivitySummaryAsync(startDate, endDate);
            return Ok(summary);
        }

        [HttpGet("suspicious")]
        public async Task<IActionResult> GetSuspiciousActivity()
        {
            var suspicious = await _auditService.GetSuspiciousActivityAsync();
            return Ok(suspicious);
        }
    }
}
