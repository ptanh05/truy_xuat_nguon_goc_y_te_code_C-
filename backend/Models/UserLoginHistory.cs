namespace PharmaDNA.Models
{
    public class UserLoginHistory
    {
        public int Id { get; set; }
        public string UserAddress { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime LoginAt { get; set; } = DateTime.UtcNow;
        public DateTime? LogoutAt { get; set; }
        public bool IsSuccessful { get; set; } = true;
        public string? UserId { get; set; }
        public DateTime? LoginTime { get; set; }
        public string? IpAddress { get; set; }
        public string? Status { get; set; }
        public string? FailureReason { get; set; }
    }
}