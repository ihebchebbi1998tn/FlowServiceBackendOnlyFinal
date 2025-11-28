using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class InstallationRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime RequestedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
        public string? TechnicianId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public Device? Device { get; set; }
    }
}