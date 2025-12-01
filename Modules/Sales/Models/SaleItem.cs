using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Modules.Sales.Models
{
    [Table("sale_items")]
    public class SaleItem
    {
        [Key]
        [Column("id")]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("sale_id")]
        [MaxLength(50)]
        public string SaleId { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;

        [Column("article_id")]
        [MaxLength(50)]
        public string ArticleId { get; set; } = string.Empty;

        [Required]
        [Column("item_name")]
        [MaxLength(255)]
        public string ItemName { get; set; } = string.Empty;

        [Column("item_code")]
        [MaxLength(100)]
        public string? ItemCode { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("quantity", TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; } = 1;

        [Required]
        [Column("unit_price", TypeName = "decimal(15,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [Column("total_price", TypeName = "decimal(15,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? TotalPrice { get; set; }

        [Column("discount", TypeName = "decimal(15,2)")]
        public decimal Discount { get; set; } = 0;

        [Column("discount_type")]
        [MaxLength(20)]
        public string DiscountType { get; set; } = "percentage";

        [Column("installation_id")]
        [MaxLength(50)]
        public string? InstallationId { get; set; }

        [Column("installation_name")]
        [MaxLength(255)]
        public string? InstallationName { get; set; }

        [Column("requires_service_order")]
        public bool RequiresServiceOrder { get; set; } = false;

        [Column("service_order_generated")]
        public bool ServiceOrderGenerated { get; set; } = false;

        [Column("service_order_id")]
        [MaxLength(50)]
        public string? ServiceOrderId { get; set; }

        [Column("fulfillment_status")]
        [MaxLength(20)]
        public string? FulfillmentStatus { get; set; }

        // Navigation Property
        public virtual Sale? Sale { get; set; }
    }
}
