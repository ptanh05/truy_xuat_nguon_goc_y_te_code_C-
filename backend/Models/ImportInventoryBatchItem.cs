namespace PharmaDNA.Models
{
    public class ImportInventoryBatchItem
    {
        public int NFTId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
