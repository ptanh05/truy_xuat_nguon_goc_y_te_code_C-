using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class AuditService : IAuditService
    {
        private readonly PharmaDNAContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(PharmaDNAContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(string entityType, int entityId, string action, string performedBy,
            string performedByName, object oldValues, object newValues, string reason = null)
        {
            try
            {
                var oldValuesJson = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
                var newValuesJson = newValues != null ? JsonSerializer.Serialize(newValues) : null;

                var changes = CalculateChanges(oldValues, newValues);
                var changesJson = JsonSerializer.Serialize(changes);

                var auditLog = new AuditLog
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    PerformedBy = performedBy,
                    PerformedByName = performedByName,
                    OldValues = oldValuesJson,
                    NewValues = newValuesJson,
                    Changes = changesJson,
                    Reason = reason,
                    Status = "Success",
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);

                // Create entity history snapshot
                var history = new EntityHistory
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityData = newValuesJson,
                    CreatedBy = performedBy,
                    CreatedDate = DateTime.UtcNow,
                    Version = await GetNextVersionAsync(entityType, entityId)
                };

                _context.EntityHistories.Add(history);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Audit log created: {entityType} {entityId} - {action} by {performedByName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging action: {ex.Message}");
                throw;
            }
        }

        public async Task LogFailedActionAsync(string entityType, int entityId, string action, 
            string performedBy, string errorMessage)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    PerformedBy = performedBy,
                    Status = "Failed",
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogWarning($"Failed action logged: {entityType} {entityId} - {action} - {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging failed action: {ex.Message}");
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string entityType = null, int? entityId = null,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetUserActivityAsync(string performedBy, DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.AuditLogs
                .Where(a => a.PerformedBy == performedBy)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<EntityHistory>> GetEntityHistoryAsync(string entityType, int entityId)
        {
            return await _context.EntityHistories
                .Where(h => h.EntityType == entityType && h.EntityId == entityId)
                .OrderBy(h => h.Version)
                .ToListAsync();
        }

        public async Task<EntityHistory> GetEntityVersionAsync(string entityType, int entityId, int version)
        {
            return await _context.EntityHistories
                .FirstOrDefaultAsync(h => h.EntityType == entityType && h.EntityId == entityId && h.Version == version);
        }

        public async Task<Dictionary<string, int>> GetActivitySummaryAsync(DateTime startDate, DateTime endDate)
        {
            var summary = new Dictionary<string, int>();

            var logs = await _context.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .ToListAsync();

            var actionCounts = logs
                .GroupBy(a => a.Action)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in actionCounts)
            {
                summary[$"Action_{kvp.Key}"] = kvp.Value;
            }

            var entityCounts = logs
                .GroupBy(a => a.EntityType)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in entityCounts)
            {
                summary[$"Entity_{kvp.Key}"] = kvp.Value;
            }

            summary["TotalActions"] = logs.Count;
            summary["SuccessfulActions"] = logs.Count(a => a.Status == "Success");
            summary["FailedActions"] = logs.Count(a => a.Status == "Failed");

            return summary;
        }

        public async Task<List<AuditLog>> GetSuspiciousActivityAsync()
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            var suspiciousLogs = await _context.AuditLogs
                .Where(a => a.Timestamp >= oneHourAgo && a.Status == "Failed")
                .GroupBy(a => a.PerformedBy)
                .Where(g => g.Count() > 5)
                .SelectMany(g => g)
                .ToListAsync();

            return suspiciousLogs;
        }

        private Dictionary<string, object> CalculateChanges(object oldValues, object newValues)
        {
            var changes = new Dictionary<string, object>();

            if (oldValues == null || newValues == null)
                return changes;

            var oldDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(oldValues));
            var newDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(newValues));

            foreach (var key in newDict.Keys)
            {
                if (!oldDict.ContainsKey(key) || !Equals(oldDict[key], newDict[key]))
                {
                    changes[key] = new
                    {
                        oldValue = oldDict.ContainsKey(key) ? oldDict[key] : null,
                        newValue = newDict[key]
                    };
                }
            }

            return changes;
        }

        private async Task<int> GetNextVersionAsync(string entityType, int entityId)
        {
            var lastVersion = await _context.EntityHistories
                .Where(h => h.EntityType == entityType && h.EntityId == entityId)
                .MaxAsync(h => (int?)h.Version) ?? 0;

            return lastVersion + 1;
        }
    }
}
