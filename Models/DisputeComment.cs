using System;

namespace PharmaDNA.Models
{
    public class DisputeComment
    {
        public int Id { get; set; }
        public int DisputeId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsInternal { get; set; } // Internal notes only visible to admins

        // Navigation properties
        public Dispute Dispute { get; set; }
        public User User { get; set; }
    }
}
