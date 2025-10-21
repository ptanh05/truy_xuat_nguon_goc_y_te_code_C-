using System;

namespace PharmaDNA.Models
{
    public class CreateNFTBatchItem
    {
        public string ProductName { get; set; }
        public string Manufacturer { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
