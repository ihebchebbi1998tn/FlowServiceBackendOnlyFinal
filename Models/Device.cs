using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class Device
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string Model { get; set; } = string.Empty;
        [Required]
        public string Reference { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Tva { get; set; }
        [Required]
        public string ImageName { get; set; } = string.Empty;
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        [Required]
        public string OrganizationId { get; set; } = string.Empty;
        [Required]
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<CustomerDevice> CustomerDevices { get; set; } = new List<CustomerDevice>();
        public ICollection<InstallationRequest> Requests { get; set; } = new List<InstallationRequest>();
    }
}