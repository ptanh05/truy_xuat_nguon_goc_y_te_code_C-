using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class BatchOperation
    {
        public int Id { get; set; }
        public string OperationType { get; set; } // CreateNFT, UpdatePrice, ImportData, etc.
        public string Status { get; set; } // Pending, Processing, Completed, Failed
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int FailedRecords { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string ErrorLog { get; set; }
        public double ProgressPercentage { get; set; }

        // Navigation properties
        public User CreatedByUser { get; set; }
        public ICollection<BatchOperationDetail> Details { get; set; } = new List<BatchOperationDetail>();
    }
}
