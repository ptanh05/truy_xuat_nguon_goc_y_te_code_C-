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

    public class AlertRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public string TriggerType { get; set; } // "LowStock", "ExpiryWarning", "TransferPending", "TransferRejected"
        public string Condition { get; set; } // JSON condition
        public string NotificationType { get; set; }
        public string NotificationChannels { get; set; } // Comma-separated: "InApp,Email,SMS"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }

    public class EmailTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string PlaceholderVariables { get; set; } // JSON list of variables
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
