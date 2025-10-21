using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class Dispute
    {
        public int Id { get; set; }
        public int NFTId { get; set; }
        public int ReportedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string DisputeType { get; set; } // Counterfeit, Damaged, Missing, Quality, Other
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Open, InProgress, Resolved, Closed, Rejected
        public string Priority { get; set; } // Low, Medium, High, Critical
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string Evidence { get; set; } // JSON array of file paths
        public decimal CompensationAmount { get; set; }
        public string CompensationStatus { get; set; } // Pending, Approved, Rejected, Paid

        // Navigation properties
        public NFT NFT { get; set; }
        public User ReportedByUser { get; set; }
        public User AssignedToUser { get; set; }
        public ICollection<DisputeComment> Comments { get; set; } = new List<DisputeComment>();
        public ICollection<DisputeResolution> Resolutions { get; set; } = new List<DisputeResolution>();
    }
}
