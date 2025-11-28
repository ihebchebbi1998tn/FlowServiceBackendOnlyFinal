using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class InvoiceItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string InvoiceId { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        public Invoice? Invoice { get; set; }
    }
}