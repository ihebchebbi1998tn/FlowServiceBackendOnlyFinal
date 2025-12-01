using System;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Modules.Dispatches.DTOs
{
    public class CreateExpenseDto
    {
        [Required]
        public string TechnicianId { get; set; } = null!;
        [Required]
        public string Type { get; set; } = null!;
        [Required]
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ExpenseDto
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string TechnicianId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class ApproveExpenseDto
    {
        public string ApprovedBy { get; set; } = null!;
    }
}
