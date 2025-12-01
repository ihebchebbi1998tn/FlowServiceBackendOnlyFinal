using System;

namespace MyApi.Modules.Dispatches.Models
{
    public class Expense
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string TechnicianId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
