using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class RefreshToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public UserConnectedDevice? Device { get; set; }
    }
}