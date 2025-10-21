namespace PharmaDNA.Models
{
    public class CreateUserRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
    }
}
