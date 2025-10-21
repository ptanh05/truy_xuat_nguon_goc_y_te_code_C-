using System;

namespace PharmaDNA.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityType { get; set; } // "NFT", "TransferRequest", "InventoryItem", "User"
        public int EntityId { get; set; }
        public string Action { get; set; } // "Create", "Update", "Delete", "Approve", "Reject"
        public string PerformedBy { get; set; } // Wallet address or user ID
        public string PerformedByName { get; set; }
        public string OldValues { get; set; } // JSON of previous values
        public string NewValues { get; set; } // JSON of new values
        public string Changes { get; set; } // JSON of what changed
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; } // Optional reason for the action
        public string Status { get; set; } // "Success", "Failed"
        public string ErrorMessage { get; set; }
    }

    public class EntityHistory
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public int Version { get; set; }
        public string EntityData { get; set; } // JSON snapshot of entity
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
    }
}
