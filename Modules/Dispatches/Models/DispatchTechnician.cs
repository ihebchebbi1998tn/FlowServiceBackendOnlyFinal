using System;

namespace MyApi.Modules.Dispatches.Models
{
    public class DispatchTechnician
    {
        public string DispatchId { get; set; } = null!;
        public string TechnicianId { get; set; } = null!;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
}
