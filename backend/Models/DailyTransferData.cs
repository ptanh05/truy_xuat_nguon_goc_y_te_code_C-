namespace PharmaDNA.Models
{
    public class DailyTransferData
    {
        public DateTime Date { get; set; }
        public int TransferCount { get; set; }
        public int SuccessfulTransfers { get; set; }
        public int FailedTransfers { get; set; }
        public decimal TotalValue { get; set; }
        public int Count { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
    }
}
