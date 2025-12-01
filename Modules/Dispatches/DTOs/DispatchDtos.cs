using System;
using System.Collections.Generic;

namespace MyApi.Modules.Dispatches.DTOs
{
    public class UserLightDto
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class SchedulingDto
    {
        public DateTime? ScheduledDate { get; set; }
        public TimeSpan? ScheduledStartTime { get; set; }
        public TimeSpan? ScheduledEndTime { get; set; }
        public int? EstimatedDuration { get; set; }
        public int? TravelTime { get; set; }
        public decimal? TravelDistance { get; set; }
    }

    public class DispatchListItemDto
    {
        public string Id { get; set; } = null!;
        public string DispatchNumber { get; set; } = null!;
        public string? JobId { get; set; }
        public string? ServiceOrderId { get; set; }
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public List<UserLightDto> AssignedTechnicians { get; set; } = new();
        public SchedulingDto? Scheduling { get; set; }
    }

    public class DispatchDto : DispatchListItemDto
    {
        public List<object> TimeEntries { get; set; } = new();
        public List<object> Expenses { get; set; } = new();
        public List<object> MaterialsUsed { get; set; } = new();
        public List<object> Attachments { get; set; } = new();
        public List<object> Notes { get; set; } = new();
        public int CompletionPercentage { get; set; }
        public string? DispatchedBy { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
