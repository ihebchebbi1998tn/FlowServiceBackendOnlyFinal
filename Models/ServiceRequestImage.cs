using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class ServiceRequestImage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string ServiceRequestId { get; set; } = string.Empty;
        [Required]
        public string ImageName { get; set; } = string.Empty;
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public ServiceRequest? ServiceRequest { get; set; }
    }
}