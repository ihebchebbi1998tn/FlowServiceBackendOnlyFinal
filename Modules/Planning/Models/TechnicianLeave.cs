using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Modules.Planning.Models
{
    [Table("technician_leaves")]
    public class TechnicianLeave
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("technician_id")]
        public int TechnicianId { get; set; }

        [Required]
        [Column("leave_type")]
        [MaxLength(50)]
        public string LeaveType { get; set; } = null!;

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("start_time")]
        public TimeSpan? StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan? EndTime { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "pending";

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("approved_by")]
        public int? ApprovedBy { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
