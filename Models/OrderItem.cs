using System.ComponentModel.DataAnnotations;

namespace ConsolidatedApi.Models
{
    public class OrderItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string OrderId { get; set; } = string.Empty;
        [Required]
        public string SparePartId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        public Order? Order { get; set; }
        public SparePart? SparePart { get; set; }
    }
}