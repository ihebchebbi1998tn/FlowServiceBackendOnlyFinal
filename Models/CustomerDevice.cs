using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class CustomerDevice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public Device? Device { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}