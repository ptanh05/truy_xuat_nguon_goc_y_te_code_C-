using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Web.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(42)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
