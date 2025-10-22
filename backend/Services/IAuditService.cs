using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string entityType, int entityId, string action, string performedBy, 
            string performedByName, object oldValues, object newValues, string reason = null);
        Task LogFailedActionAsync(string entityType, int entityId, string action, string performedBy, 
            string errorMessage);
        Task<List<AuditLog>> GetAuditLogsAsync(string entityType = null, int? entityId = null, 
            DateTime? startDate = null, DateTime? endDate = null);
        Task<List<AuditLog>> GetUserActivityAsync(string performedBy, DateTime? startDate = null, 
            DateTime? endDate = null);
        Task<List<EntityHistory>> GetEntityHistoryAsync(string entityType, int entityId);
        Task<EntityHistory> GetEntityVersionAsync(string entityType, int entityId, int version);
        Task<Dictionary<string, int>> GetActivitySummaryAsync(DateTime startDate, DateTime endDate);
        Task<List<AuditLog>> GetSuspiciousActivityAsync();
    }
}
