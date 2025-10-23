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
    public class ReportService : IReportService
    {
        private readonly PharmaDNAContext _context;

        public ReportService(PharmaDNAContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsData> GetAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var nfts = await _context.NFTs.ToListAsync();
            var transfers = await _context.TransferRequests
                .Where(t => t.RequestDate >= startDate && t.RequestDate <= endDate)
                .ToListAsync();

            var totalNFTs = nfts.Count;
            var totalTransfers = transfers.Count;
            var successfulTransfers = transfers.Count(t => t.Status == "Approved");
            var rejectedTransfers = transfers.Count(t => t.Status == "Rejected");
            var pendingTransfers = transfers.Count(t => t.Status == "Pending");

            var successRate = totalTransfers > 0 ? (decimal)successfulTransfers / totalTransfers * 100 : 0;

            var transfersByManufacturer = nfts
                .GroupBy(n => n.Manufacturer)
                .ToDictionary(g => g.Key, g => g.Count());

            var transfersByStatus = transfers
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var dailyTransfers = transfers
                .GroupBy(t => t.RequestDate?.Date ?? DateTime.UtcNow.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyTransferData
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Successful = g.Count(t => t.Status == "Approved"),
                    Failed = g.Count(t => t.Status == "Rejected")
                })
                .ToList();

            var topProducts = nfts
                .GroupBy(n => n.ProductName)
                .Select(g => new ProductAnalytics
                {
                    ProductName = g.Key,
                    TotalCreated = g.Count(),
                    TotalTransferred = transfers.Count(t => t.NFT.ProductName == g.Key),
                    CurrentStock = (int)g.Sum(n => n.Quantity ?? 0),
                    TransferRate = g.Count() > 0 ? (double)transfers.Count(t => t.NFT.ProductName == g.Key) / g.Count() * 100 : 0
                })
                .OrderByDescending(p => p.TotalTransferred)
                .Take(10)
                .ToList();

            return new AnalyticsData
            {
                TotalNFTsCreated = totalNFTs,
                TotalTransfers = totalTransfers,
                SuccessfulTransfers = successfulTransfers,
                RejectedTransfers = rejectedTransfers,
                PendingTransfers = pendingTransfers,
                SuccessRate = (double)successRate,
                TransfersByManufacturer = System.Text.Json.JsonSerializer.Serialize(transfersByManufacturer),
                TransfersByStatus = System.Text.Json.JsonSerializer.Serialize(transfersByStatus),
                DailyTransfers = System.Text.Json.JsonSerializer.Serialize(dailyTransfers),
                TopProducts = System.Text.Json.JsonSerializer.Serialize(topProducts)
            };
        }

        public async Task<ReportData> GenerateReportAsync(string reportType, DateTime startDate, DateTime endDate, string generatedBy)
        {
            var analytics = await GetAnalyticsAsync(startDate, endDate);
            var content = JsonSerializer.Serialize(analytics);

            var report = new ReportData
            {
                ReportType = reportType,
                ReportName = $"{reportType} Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                StartDate = startDate,
                EndDate = endDate,
                GeneratedBy = generatedBy,
                Content = content,
                IsPublished = false
            };

            _context.ReportData.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<List<ReportData>> GetReportsAsync()
        {
            return await _context.ReportData
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();
        }

        public async Task<ReportData> GetReportAsync(int id)
        {
            return await _context.ReportData.FindAsync(id);
        }

        public async Task<byte[]> ExportReportToPdfAsync(int reportId)
        {
            var report = await GetReportAsync(reportId);
            if (report == null) return null;

            // This is a placeholder - implement actual PDF generation
            var pdfContent = System.Text.Encoding.UTF8.GetBytes(report.Content);
            return pdfContent;
        }

        public async Task<byte[]> ExportReportToExcelAsync(int reportId)
        {
            var report = await GetReportAsync(reportId);
            if (report == null) return null;

            // This is a placeholder - implement actual Excel generation
            var excelContent = System.Text.Encoding.UTF8.GetBytes(report.Content);
            return excelContent;
        }

        public async Task<Dictionary<string, int>> GetTransfersByStatusAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TransferRequests
                .Where(t => t.RequestDate >= startDate && t.RequestDate <= endDate)
                .GroupBy(t => t.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetTransfersByManufacturerAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TransferRequests
                .Where(t => t.RequestDate >= startDate && t.RequestDate <= endDate)
                .Include(t => t.NFT)
                .GroupBy(t => t.NFT.Manufacturer)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<List<ProductAnalytics>> GetProductAnalyticsAsync()
        {
            var nfts = await _context.NFTs.ToListAsync();
            var transfers = await _context.TransferRequests.ToListAsync();

            return nfts
                .GroupBy(n => n.ProductName)
                .Select(g => new ProductAnalytics
                {
                    ProductName = g.Key,
                    TotalCreated = g.Count(),
                    TotalTransferred = transfers.Count(t => t.NFT.ProductName == g.Key),
                    CurrentStock = (int)g.Sum(n => n.Quantity ?? 0),
                    TransferRate = g.Count() > 0 ? (double)transfers.Count(t => t.NFT.ProductName == g.Key) / g.Count() * 100 : 0
                })
                .OrderByDescending(p => p.TotalTransferred)
                .ToList();
        }
    }
}
