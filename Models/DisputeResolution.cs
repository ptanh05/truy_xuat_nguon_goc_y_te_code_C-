using System;

namespace PharmaDNA.Models
{
    public class DisputeResolution
    {
        public int Id { get; set; }
        public int DisputeId { get; set; }
        public int ResolvedByUserId { get; set; }
        public string ResolutionType { get; set; } // Refund, Replacement, Compensation, Rejection
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } // Pending, Approved, Executed

        // Navigation properties
        public Dispute Dispute { get; set; }
        public User ResolvedByUser { get; set; }
    }
}
