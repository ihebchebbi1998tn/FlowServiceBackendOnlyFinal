using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class SparePart
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PartNumber { get; set; }
        public string? Category { get; set; }
        public int StockQuantity { get; set; }
        [Required]
        public string OrganizationId { get; set; } = string.Empty;
        public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}