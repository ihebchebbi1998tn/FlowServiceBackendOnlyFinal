using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string OrderNumber { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal? DeliveryCosts { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? ShippingAddress { get; set; }
        
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}