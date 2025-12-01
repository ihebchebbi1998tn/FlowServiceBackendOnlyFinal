using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Modules.Sales.Models
{
    [Table("sale_activities")]
    public class SaleActivity
    {
        [Key]
        [Column("id")]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("sale_id")]
        [MaxLength(50)]
        public string SaleId { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Column("description")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column("details")]
        public string? Details { get; set; }

        [Column("old_value")]
        public string? OldValue { get; set; }

        [Column("new_value")]
        public string? NewValue { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("created_by")]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_by_name")]
        [MaxLength(255)]
        public string CreatedByName { get; set; } = string.Empty;

        // Navigation Property
        public virtual Sale? Sale { get; set; }
    }
}
