using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsFromUser { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public string? OrganizationId { get; set; }
        public string? SessionId { get; set; }
    }
}