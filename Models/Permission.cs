using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Module { get; set; } // e.g., "NFT", "Transfer", "Inventory"
        public string Action { get; set; } // e.g., "Create", "Read", "Update", "Delete"
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
