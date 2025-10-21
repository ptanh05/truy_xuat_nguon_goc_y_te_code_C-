using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class InventoryLocation
    {
        public int Id { get; set; }
        public string LocationName { get; set; }
        public string LocationCode { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int Capacity { get; set; }
        public int CurrentStock { get; set; }
        public string ManagerName { get; set; }
        public string ManagerEmail { get; set; }
        public string ManagerPhone { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }

    public class InventoryItem
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string StorageCondition { get; set; } // e.g., "2-8°C", "Room Temperature"
        public string Batch { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public virtual NFT NFT { get; set; }
        public virtual InventoryLocation Location { get; set; }
        public virtual ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
    }

    public class InventoryMovement
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public string MovementType { get; set; } // "In", "Out", "Transfer", "Adjustment"
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public string Reference { get; set; } // Transfer ID, Order ID, etc.
        public string PerformedBy { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; }

        public virtual InventoryItem InventoryItem { get; set; }
    }

    public class InventoryAlert
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public string AlertType { get; set; } // "LowStock", "ExpiryWarning", "OverStock"
        public string Severity { get; set; } // "Low", "Medium", "High"
        public string Message { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedDate { get; set; }

        public virtual InventoryItem InventoryItem { get; set; }
    }
}
