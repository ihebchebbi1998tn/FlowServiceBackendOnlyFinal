using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class UserConnectedDevice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string DeviceName { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? DeviceToken { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActiveAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ApplicationUser? User { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}