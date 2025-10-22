using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IReportService
    {
        Task<AnalyticsData> GetAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<ReportData> GenerateReportAsync(string reportType, DateTime startDate, DateTime endDate, string generatedBy);
        Task<List<ReportData>> GetReportsAsync();
        Task<ReportData> GetReportAsync(int id);
        Task<byte[]> ExportReportToPdfAsync(int reportId);
        Task<byte[]> ExportReportToExcelAsync(int reportId);
        Task<Dictionary<string, int>> GetTransfersByStatusAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetTransfersByManufacturerAsync(DateTime startDate, DateTime endDate);
        Task<List<ProductAnalytics>> GetProductAnalyticsAsync();
    }
}
