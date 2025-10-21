namespace PharmaDNA.Models
{
    public class DisputeStatistics
    {
        public int TotalDisputes { get; set; }
        public int OpenDisputes { get; set; }
        public int ResolvedDisputes { get; set; }
        public double ResolutionRate { get; set; }
        public decimal TotalCompensation { get; set; }
    }
}
