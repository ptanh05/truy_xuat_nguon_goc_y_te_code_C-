namespace PharmaDNA.Web.Models.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}
