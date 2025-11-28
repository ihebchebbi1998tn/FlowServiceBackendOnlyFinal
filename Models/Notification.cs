using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class Notification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Message { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        public string? Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public string? ActionUrl { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }
}