using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class ServiceRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal? Charges { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        public Device? Device { get; set; }
        public Feedback? Feedback { get; set; }
        public ICollection<ServiceRequestImage> Images { get; set; } = new List<ServiceRequestImage>();
    }
}