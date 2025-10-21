using System;

namespace PharmaDNA.Models
{
    public class BatchOperationDetail
    {
        public int Id { get; set; }
        public int BatchOperationId { get; set; }
        public int RecordNumber { get; set; }
        public string Status { get; set; } // Success, Failed
        public string Data { get; set; } // JSON data
        public string ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }

        // Navigation properties
        public BatchOperation BatchOperation { get; set; }
    }
}
