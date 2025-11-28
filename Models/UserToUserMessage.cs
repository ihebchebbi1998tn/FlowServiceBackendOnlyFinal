using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class UserToUserMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public string SenderId { get; set; } = string.Empty;
        [Required]
        public string ChatId { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        public UserToUserChat? Chat { get; set; }
    }
}