using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IAnalyticsService
    {
        // Dashboard Metrics
        Task<DashboardMetrics> GetDashboardMetricsAsync();
        Task<IEnumerable<TrendData>> GetNFTCreationTrendAsync(int days = 30);
        Task<IEnumerable<TrendData>> GetTransferTrendAsync(int days = 30);
        Task<IEnumerable<TrendData>> GetRevenueTrendAsync(int days = 30);

        // Category Analysis
        Task<IEnumerable<CategoryAnalysis>> GetNFTByManufacturerAsync();
        Task<IEnumerable<CategoryAnalysis>> GetTransferByStatusAsync();
        Task<IEnumerable<CategoryAnalysis>> GetInventoryByLocationAsync();

        // Anomaly Detection
        Task<IEnumerable<AnomalyAlert>> GetAnomalyAlertsAsync();
        Task<bool> CreateAnomalyAlertAsync(AnomalyAlert alert);
        Task<bool> ResolveAnomalyAlertAsync(int alertId);

        // Predictive Analytics
        Task<IEnumerable<PredictiveAnalysis>> GetPredictiveAnalysisAsync(int days = 30);

        // Custom Reports
        Task<byte[]> GenerateCustomReportAsync(string reportType, DateTime startDate, DateTime endDate);
        Task<IEnumerable<dynamic>> GetCustomAnalyticsAsync(string query);

        // Performance Metrics
        Task<Dictionary<string, object>> GetPerformanceMetricsAsync();
        Task<Dictionary<string, object>> GetUserBehaviorAnalyticsAsync();
    }
}
