using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class AnalyticsData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string MetricType { get; set; } // NFTCreated, TransferCompleted, InventoryMovement, etc.
        public int Value { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Details { get; set; } // JSON
    }

    public class DashboardMetrics
    {
        public int TotalNFTs { get; set; }
        public int TotalTransfers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal TotalValue { get; set; }
        public double AverageTransferTime { get; set; }
        public int DisputeCount { get; set; }
        public double DisputeResolutionRate { get; set; }
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class CategoryAnalysis
    {
        public string Category { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AnomalyAlert
    {
        public int Id { get; set; }
        public string AlertType { get; set; } // UnusualActivity, PriceSpike, HighDispute, etc.
        public string Description { get; set; }
        public DateTime DetectedAt { get; set; }
        public string Severity { get; set; } // Low, Medium, High, Critical
        public bool IsResolved { get; set; }
    }

    public class PredictiveAnalysis
    {
        public DateTime Date { get; set; }
        public int PredictedNFTCreations { get; set; }
        public int PredictedTransfers { get; set; }
        public decimal PredictedRevenue { get; set; }
        public double Confidence { get; set; }
    }
}
