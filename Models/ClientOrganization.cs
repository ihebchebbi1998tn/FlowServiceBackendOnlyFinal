using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class ClientOrganization
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string EmailOrganisation { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string PrimaryColor { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string SecondaryColor { get; set; } = string.Empty;
        [Required]
        public string LogoName { get; set; } = string.Empty;
        [Required]
        public string LogoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsBgRemoved { get; set; } = false;
        public string? OnlinePaymentSolutions { get; set; }
        public string? Description { get; set; }
        public string? StreetName { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        
        public ICollection<UiPage> UiPages { get; set; } = new List<UiPage>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}