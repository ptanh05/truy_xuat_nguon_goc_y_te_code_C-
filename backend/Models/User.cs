namespace PharmaDNA.Models
{
    public class User
    {
        public int Id { get; set; }
        public string WalletAddress { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Manufacturer, Distributor, Pharmacy, Admin
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public string? PasswordHash { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
