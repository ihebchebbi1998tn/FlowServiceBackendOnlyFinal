using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Modules.Dispatches.DTOs
{
    public class CreateDispatchFromJobDto
    {
        [Required]
        [MinLength(1)]
        public List<string> AssignedTechnicianIds { get; set; } = new();

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public TimeSpan ScheduledStartTime { get; set; }

        [Required]
        public TimeSpan ScheduledEndTime { get; set; }

        public int? EstimatedTravelTime { get; set; }
        public decimal? EstimatedTravelDistance { get; set; }
        [Required]
        public string Priority { get; set; } = "medium";
        public string? Notes { get; set; }
    }
}
