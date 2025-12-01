using System;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Modules.Dispatches.DTOs
{
    public class CreateTimeEntryDto
    {
        [Required]
        public string TechnicianId { get; set; } = null!;
        [Required]
        public string WorkType { get; set; } = "work";
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        public string? Description { get; set; }
        public bool Billable { get; set; }
        public decimal? HourlyRate { get; set; }
    }

    public class TimeEntryDto
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string TechnicianId { get; set; } = null!;
        public string WorkType { get; set; } = null!;
        public int Duration { get; set; }
        public decimal? TotalCost { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class ApproveTimeEntryDto
    {
        public string ApprovedBy { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
