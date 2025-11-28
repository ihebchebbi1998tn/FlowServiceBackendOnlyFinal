using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class Invoice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}