using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class Role : IdentityRole
    {
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public string? OrganizationId { get; set; }
        public ClientOrganization? ClientOrganization { get; set; }
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}