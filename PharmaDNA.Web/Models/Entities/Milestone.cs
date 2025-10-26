using System.ComponentModel.DataAnnotations;

namespace PharmaDNA.Web.Models.Entities
{
    public class Milestone
    {
        public int Id { get; set; }
        
        public int NftId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [MaxLength(255)]
        public string? Location { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(42)]
        public string ActorAddress { get; set; } = string.Empty;
        
        // Navigation properties
        public NFT NFT { get; set; } = null!;
    }
}
