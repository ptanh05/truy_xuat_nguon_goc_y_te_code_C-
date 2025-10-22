namespace PharmaDNA.Models
{
    public class TransferBatchItem
    {
        public int NFTId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public int Quantity { get; set; }
    }
}
