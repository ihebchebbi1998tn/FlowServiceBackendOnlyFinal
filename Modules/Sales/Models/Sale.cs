using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace MyApi.Modules.Sales.Models
{
    [Table("sales")]
    public class Sale
    {
        [Key]
        [Column("id")]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        // Contact Information
        [Required]
        [Column("contact_id")]
        public int ContactId { get; set; }

        // Financial Information
        [Required]
        [Column("amount", TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; } = 0;

        [Required]
        [Column("currency")]
        [MaxLength(3)]
        public string Currency { get; set; } = "TND";

        [Column("taxes", TypeName = "decimal(15,2)")]
        public decimal? Taxes { get; set; } = 0;

        [Column("discount", TypeName = "decimal(15,2)")]
        public decimal? Discount { get; set; } = 0;

        [Column("total_amount", TypeName = "decimal(15,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? TotalAmount { get; set; }

        // Status & Classification
        [Required]
        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "won";

        [Column("stage")]
        [MaxLength(50)]
        public string? Stage { get; set; } = "closed";

        [Column("priority")]
        [MaxLength(20)]
        public string? Priority { get; set; }

        [Column("category")]
        [MaxLength(50)]
        public string? Category { get; set; }

        [Column("source")]
        [MaxLength(50)]
        public string? Source { get; set; }

        // Billing Address
        [Column("billing_address")]
        public string? BillingAddress { get; set; }

        [Column("billing_postal_code")]
        [MaxLength(20)]
        public string? BillingPostalCode { get; set; }

        [Column("billing_country")]
        [MaxLength(100)]
        public string? BillingCountry { get; set; }

        // Delivery Address
        [Column("delivery_address")]
        public string? DeliveryAddress { get; set; }

        [Column("delivery_postal_code")]
        [MaxLength(20)]
        public string? DeliveryPostalCode { get; set; }

        [Column("delivery_country")]
        [MaxLength(100)]
        public string? DeliveryCountry { get; set; }

        // Dates
        [Column("estimated_close_date")]
        public DateTime? EstimatedCloseDate { get; set; }

        [Column("actual_close_date")]
        public DateTime? ActualCloseDate { get; set; }

        [Column("valid_until")]
        public DateTime? ValidUntil { get; set; }

        // Assignment
        [Column("assigned_to")]
        [MaxLength(50)]
        public string? AssignedTo { get; set; }

        [Column("assigned_to_name")]
        [MaxLength(255)]
        public string? AssignedToName { get; set; }

        // Tags
        [Column("tags")]
        public string[]? Tags { get; set; }

        // Timestamps
        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("created_by")]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("last_activity")]
        public DateTime? LastActivity { get; set; }

        // Link back to original offer
        [Column("offer_id")]
        [MaxLength(50)]
        public string? OfferId { get; set; }

        // Conversion tracking and fulfillment
        [Column("converted_from_offer_at")]
        public DateTime? ConvertedFromOfferAt { get; set; }

        [Column("lost_reason")]
        public string? LostReason { get; set; }

        [Column("materials_fulfillment")]
        [MaxLength(20)]
        public string? MaterialsFulfillment { get; set; }

        [Column("service_orders_status")]
        [MaxLength(20)]
        public string? ServiceOrdersStatus { get; set; }

        // Navigation Properties
        public virtual ICollection<SaleItem>? Items { get; set; }
        public virtual ICollection<SaleActivity>? Activities { get; set; }
    }
}
