using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class UiPage
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string OrganizationId { get; set; } = string.Empty;
        public List<string> FieldsToNotDisplay { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ClientOrganization? ClientOrganization { get; set; }
    }
}