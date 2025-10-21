namespace PharmaDNA.Models
{
    public class User
    {
        public int Id { get; set; }
        public string WalletAddress { get; set; }
        public string Role { get; set; } // Manufacturer, Distributor, Pharmacy, Admin
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
