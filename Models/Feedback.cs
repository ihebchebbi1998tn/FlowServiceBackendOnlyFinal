using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class Feedback
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = string.Empty;
        public string? ServiceRequestId { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ServiceRequest? ServiceRequest { get; set; }
    }
}