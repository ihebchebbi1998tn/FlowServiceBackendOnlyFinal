using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class UserToUserChat
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string User1Id { get; set; } = string.Empty;
        [Required]
        public string User2Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string? UserIdWhoDeleted { get; set; }
        
        public ICollection<UserToUserMessage> Messages { get; set; } = new List<UserToUserMessage>();
    }
}