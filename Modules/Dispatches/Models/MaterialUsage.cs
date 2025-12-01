using System;

namespace MyApi.Modules.Dispatches.Models
{
    public class MaterialUsage
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string ArticleId { get; set; } = null!;
        public string? ArticleName { get; set; }
        public string? Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string UsedBy { get; set; } = null!;
        public DateTime UsedAt { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
