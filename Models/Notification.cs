using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientEmail { get; set; }
        public string Type { get; set; } // transfer_request, transfer_approved, low_stock, expiry_warning, etc.
        public string Title { get; set; }
        public string Message { get; set; }
        public string Data { get; set; } // JSON
        public bool IsRead { get; set; } = false;
        public string Channel { get; set; } // "InApp", "Email", "SMS"
        public string Priority { get; set; } // "Low", "Medium", "High"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsSent { get; set; } = false;
    }

}
