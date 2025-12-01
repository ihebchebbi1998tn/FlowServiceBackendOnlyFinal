using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Modules.ServiceOrders.Models
{
    [Table("service_orders")]
    public class ServiceOrder
    {
        [Key]
        [Column("id")]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("order_number")]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [Column("sale_id")]
        [MaxLength(50)]
        public string SaleId { get; set; } = string.Empty;

        [Required]
        [Column("offer_id")]
        [MaxLength(50)]
        public string OfferId { get; set; } = string.Empty;

        [Required]
        [Column("contact_id")]
        public int ContactId { get; set; }

        [Required]
        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "draft";

        [Column("priority")]
        [MaxLength(20)]
        public string? Priority { get; set; } = "medium";

        [Column("description")]
        public string? Description { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("target_completion_date")]
        public DateTime? TargetCompletionDate { get; set; }

        [Column("actual_start_date")]
        public DateTime? ActualStartDate { get; set; }

        [Column("actual_completion_date")]
        public DateTime? ActualCompletionDate { get; set; }

        [Column("estimated_duration")]
        public int? EstimatedDuration { get; set; }

        [Column("actual_duration")]
        public int? ActualDuration { get; set; }

        [Column("estimated_cost", TypeName = "decimal(15,2)")]
        public decimal? EstimatedCost { get; set; } = 0;

        [Column("actual_cost", TypeName = "decimal(15,2)")]
        public decimal? ActualCost { get; set; } = 0;

        [Column("discount", TypeName = "decimal(15,2)")]
        public decimal? Discount { get; set; } = 0;

        [Column("discount_percentage", TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; } = 0;

        [Column("tax", TypeName = "decimal(15,2)")]
        public decimal? Tax { get; set; } = 0;

        [Column("total_amount", TypeName = "decimal(15,2)")]
        public decimal? TotalAmount { get; set; } = 0;

        [Column("payment_status")]
        [MaxLength(20)]
        public string? PaymentStatus { get; set; } = "pending";

        [Column("payment_terms")]
        [MaxLength(20)]
        public string? PaymentTerms { get; set; } = "net30";

        [Column("invoice_number")]
        [MaxLength(50)]
        public string? InvoiceNumber { get; set; }

        [Column("invoice_date")]
        public DateTime? InvoiceDate { get; set; }

        [Column("completion_percentage")]
        public int? CompletionPercentage { get; set; } = 0;

        [Column("requires_approval")]
        public bool RequiresApproval { get; set; } = false;

        [Column("approved_by")]
        [MaxLength(50)]
        public string? ApprovedBy { get; set; }

        [Column("approval_date")]
        public DateTime? ApprovalDate { get; set; }

        [Column("cancellation_reason")]
        public string? CancellationReason { get; set; }

        [Column("cancellation_notes")]
        public string? CancellationNotes { get; set; }

        [Column("tags")]
        public string[]? Tags { get; set; }

        [Column("custom_fields", TypeName = "jsonb")]
        public string? CustomFields { get; set; }

        [Required]
        [Column("created_by")]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_by")]
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<ServiceOrderJob>? Jobs { get; set; }
    }
}
