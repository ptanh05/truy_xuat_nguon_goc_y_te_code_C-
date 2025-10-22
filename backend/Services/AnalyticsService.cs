using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly PharmaDNAContext _context;

        public AnalyticsService(PharmaDNAContext context)
        {
            _context = context;
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync()
        {
            var totalNFTs = await _context.NFTs.CountAsync();
            var totalTransfers = await _context.TransferRequests.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalValue = await _context.NFTs.SumAsync(n => n.Price * n.Quantity);
            var disputes = await _context.Disputes.CountAsync();
            var resolvedDisputes = await _context.Disputes.CountAsync(d => d.Status == "Resolved");

            var avgTransferTime = await _context.TransferRequests
                .Where(t => t.CompletedAt.HasValue)
                .AverageAsync(t => EF.Functions.DateDiffDay(t.CreatedAt, t.CompletedAt.Value));

            return new DashboardMetrics
            {
                TotalNFTs = totalNFTs,
                TotalTransfers = totalTransfers,
                ActiveUsers = activeUsers,
                TotalValue = totalValue ?? 0,
                AverageTransferTime = avgTransferTime ?? 0,
                DisputeCount = disputes,
                DisputeResolutionRate = disputes > 0 ? (double)resolvedDisputes / disputes * 100 : 0
            };
        }

        public async Task<IEnumerable<TrendData>> GetNFTCreationTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.NFTs
                .Where(n => n.CreatedAt >= startDate)
                .GroupBy(n => n.CreatedAt.Date)
                .Select(g => new TrendData
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(n => n.Price * n.Quantity)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TrendData>> GetTransferTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.TransferRequests
                .Where(t => t.CreatedAt >= startDate)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new TrendData
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(t => t.NFT.Price * t.Quantity)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TrendData>> GetRevenueTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.TransferRequests
                .Where(t => t.CreatedAt >= startDate && t.Status == "Completed")
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new TrendData
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(t => t.NFT.Price * t.Quantity)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryAnalysis>> GetNFTByManufacturerAsync()
        {
            var total = await _context.NFTs.CountAsync();
            return await _context.NFTs
                .GroupBy(n => n.Manufacturer)
                .Select(g => new CategoryAnalysis
                {
                    Category = g.Key,
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / total * 100
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryAnalysis>> GetTransferByStatusAsync()
        {
            var total = await _context.TransferRequests.CountAsync();
            return await _context.TransferRequests
                .GroupBy(t => t.Status)
                .Select(g => new CategoryAnalysis
                {
                    Category = g.Key,
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / total * 100
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryAnalysis>> GetInventoryByLocationAsync()
        {
            var total = await _context.InventoryLocations.SumAsync(i => i.Quantity);
            return await _context.InventoryLocations
                .GroupBy(i => i.LocationName)
                .Select(g => new CategoryAnalysis
                {
                    Category = g.Key,
                    Count = g.Sum(i => i.Quantity),
                    Percentage = (decimal)g.Sum(i => i.Quantity) / total * 100
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnomalyAlert>> GetAnomalyAlertsAsync()
        {
            return await _context.AnomalyAlerts
                .OrderByDescending(a => a.DetectedAt)
                .ToListAsync();
        }

        public async Task<bool> CreateAnomalyAlertAsync(AnomalyAlert alert)
        {
            alert.DetectedAt = DateTime.UtcNow;
            _context.AnomalyAlerts.Add(alert);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResolveAnomalyAlertAsync(int alertId)
        {
            var alert = await _context.AnomalyAlerts.FindAsync(alertId);
            if (alert == null) return false;

            alert.IsResolved = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PredictiveAnalysis>> GetPredictiveAnalysisAsync(int days = 30)
        {
            var predictions = new List<PredictiveAnalysis>();
            var historicalData = await GetNFTCreationTrendAsync(days);

            var avgDaily = historicalData.Average(t => t.Count);
            var avgRevenue = historicalData.Average(t => t.Amount);

            for (int i = 1; i <= 7; i++)
            {
                predictions.Add(new PredictiveAnalysis
                {
                    Date = DateTime.UtcNow.AddDays(i),
                    PredictedNFTCreations = (int)(avgDaily * (1 + (i * 0.05))),
                    PredictedTransfers = (int)(avgDaily * 0.8 * (1 + (i * 0.03))),
                    PredictedRevenue = avgRevenue * (1 + (i * 0.04m)),
                    Confidence = 0.85 - (i * 0.05)
                });
            }

            return predictions;
        }

        public async Task<byte[]> GenerateCustomReportAsync(string reportType, DateTime startDate, DateTime endDate)
        {
            // Placeholder for PDF generation
            return new byte[] { };
        }

        public async Task<IEnumerable<dynamic>> GetCustomAnalyticsAsync(string query)
        {
            // Placeholder for custom query execution
            return new List<dynamic>();
        }

        public async Task<Dictionary<string, object>> GetPerformanceMetricsAsync()
        {
            var metrics = new Dictionary<string, object>
            {
                { "AverageResponseTime", 150 }, // ms
                { "SystemUptime", 99.9 }, // %
                { "DatabaseQueryTime", 45 }, // ms
                { "CacheHitRate", 87.5 }, // %
                { "ErrorRate", 0.1 } // %
            };

            return metrics;
        }

        public async Task<Dictionary<string, object>> GetUserBehaviorAnalyticsAsync()
        {
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalLogins = await _context.UserLoginHistories.CountAsync();
            var avgSessionDuration = 45; // minutes

            return new Dictionary<string, object>
            {
                { "ActiveUsers", activeUsers },
                { "TotalLogins", totalLogins },
                { "AverageSessionDuration", avgSessionDuration },
                { "PeakHour", "14:00-15:00" },
                { "MostActiveDay", "Wednesday" }
            };
        }
    }
}
