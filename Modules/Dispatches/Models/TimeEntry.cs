using System;

namespace MyApi.Modules.Dispatches.Models
{
    public class TimeEntry
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string TechnicianId { get; set; } = null!;
        public string WorkType { get; set; } = "work";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public string? Description { get; set; }
        public bool Billable { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal? TotalCost { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
