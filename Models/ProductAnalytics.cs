namespace PharmaDNA.Models
{
    public class ProductAnalytics
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int TotalNFTs { get; set; }
        public int TotalTransfers { get; set; }
        public decimal TotalValue { get; set; }
        public double AverageTransferTime { get; set; }
        public int DisputeCount { get; set; }
        public double DisputeResolutionRate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
