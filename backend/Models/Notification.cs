using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string RecipientAddress { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // transfer_request, transfer_approved, low_stock, expiry_warning, etc.
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty; // JSON
        public bool IsRead { get; set; } = false;
        public string Channel { get; set; } = string.Empty; // "InApp", "Email", "SMS"
        public string Priority { get; set; } = string.Empty; // "Low", "Medium", "High"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsSent { get; set; } = false;
    }

}
