using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Modules.Installations.Models
{
    [Table("maintenance_histories")]
    public class MaintenanceHistory
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("installation_id")]
        public string InstallationId { get; set; } = string.Empty;

        [Column("maintenance_date")]
        public DateTime MaintenanceDate { get; set; }

        [Column("maintenance_type")]
        [MaxLength(50)]
        public string MaintenanceType { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column("technician")]
        [MaxLength(255)]
        public string? Technician { get; set; }

        [Column("duration")]
        public int? Duration { get; set; } // in minutes

        [Column("notes")]
        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Column("next_scheduled_date")]
        public DateTime? NextScheduledDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("created_by")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        // Navigation property
        public virtual Installation? Installation { get; set; }
    }
}
