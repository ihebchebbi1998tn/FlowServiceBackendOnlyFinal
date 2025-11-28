using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public string? RoleId { get; set; }
        public Role? Role { get; set; }
        
        public string? OrganizationId { get; set; }
        public ClientOrganization? ClientOrganization { get; set; }
        
        public Address? Address { get; set; }
        public ICollection<UserConnectedDevice> ConnectedDevices { get; set; } = new List<UserConnectedDevice>();
    }
}