using System;

namespace PharmaDNA.Models
{
    public class UserLoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Status { get; set; } // Success, Failed, Locked
        public string FailureReason { get; set; }

        // Navigation properties
        public User User { get; set; }
    }
}
