using System;
using System.Collections.Generic;

namespace MyApi.Modules.Dispatches.Models
{
    public class Dispatch
    {
        public string Id { get; set; } = null!;
        public string DispatchNumber { get; set; } = null!;
        public string? ServiceOrderId { get; set; }
        public string? JobId { get; set; }

        public string Status { get; set; } = "pending";
        public string Priority { get; set; } = "medium";

        public List<DispatchTechnician> AssignedTechnicians { get; set; } = new();
        public List<string> RequiredSkills { get; set; } = new();

        public DateTime? ScheduledDate { get; set; }
        public TimeSpan? ScheduledStartTime { get; set; }
        public TimeSpan? ScheduledEndTime { get; set; }
        public int? EstimatedDuration { get; set; }

        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public int? ActualDuration { get; set; }

        public string? WorkLocationJson { get; set; }

        public int CompletionPercentage { get; set; }
        public string? DispatchedBy { get; set; }
        public DateTime? DispatchedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public List<TimeEntry> TimeEntries { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
        public List<MaterialUsage> MaterialsUsed { get; set; } = new();
        public List<Attachment> Attachments { get; set; } = new();
        public List<Note> Notes { get; set; } = new();
    }
}
