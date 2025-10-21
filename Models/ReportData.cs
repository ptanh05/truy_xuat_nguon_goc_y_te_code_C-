using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class ReportData
    {
        public int Id { get; set; }
        public string ReportType { get; set; } // "Daily", "Weekly", "Monthly", "Custom"
        public string ReportName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public string Content { get; set; } // JSON content
        public string FileUrl { get; set; } // PDF/Excel file URL
        public bool IsPublished { get; set; }
    }

    public class AnalyticsData
    {
        public int TotalNFTsCreated { get; set; }
        public int TotalTransfers { get; set; }
        public int SuccessfulTransfers { get; set; }
        public int RejectedTransfers { get; set; }
        public int PendingTransfers { get; set; }
        public decimal SuccessRate { get; set; }
        public Dictionary<string, int> TransfersByManufacturer { get; set; }
        public Dictionary<string, int> TransfersByStatus { get; set; }
        public List<DailyTransferData> DailyTransfers { get; set; }
        public List<ProductAnalytics> TopProducts { get; set; }
    }

    public class DailyTransferData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
    }

    public class ProductAnalytics
    {
        public string ProductName { get; set; }
        public int TotalCreated { get; set; }
        public int TotalTransferred { get; set; }
        public int CurrentStock { get; set; }
        public decimal TransferRate { get; set; }
    }
}
