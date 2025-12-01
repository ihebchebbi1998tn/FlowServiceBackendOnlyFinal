using System;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Modules.Dispatches.DTOs
{
    public class CreateMaterialUsageDto
    {
        [Required]
        public string ArticleId { get; set; } = null!;
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string UsedBy { get; set; } = null!;
        public string? InternalComment { get; set; }
        public bool? Replacing { get; set; }
        public string? OldArticleModel { get; set; }
    }

    public class MaterialDto
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string ArticleId { get; set; } = null!;
        public string? ArticleName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class ApproveMaterialDto
    {
        public string ApprovedBy { get; set; } = null!;
    }
}
