namespace MyApi.Modules.Sales.DTOs
{
    public class SaleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Contact Information
        public int ContactId { get; set; }
        public ContactSummaryDto? Contact { get; set; }
        
        // Financial Information
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TND";
        public decimal? Taxes { get; set; }
        public decimal? Discount { get; set; }
        public decimal? TotalAmount { get; set; }
        
        // Status & Classification
        public string Status { get; set; } = "won";
        public string? Stage { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public string? Source { get; set; }
        
        // Addresses
        public string? BillingAddress { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public string? DeliveryCountry { get; set; }
        
        // Dates
        public DateTime? EstimatedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        
        // Assignment
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        
        // Tags
        public string[]? Tags { get; set; }
        
        // Items
        public List<SaleItemDto>? Items { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastActivity { get; set; }
        
        // Link to original offer
        public string? OfferId { get; set; }

        public DateTime? ConvertedFromOfferAt { get; set; }
        public string? LostReason { get; set; }
        public string? MaterialsFulfillment { get; set; }
        public string? ServiceOrdersStatus { get; set; }
    }

    public class ContactSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class SaleItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string SaleId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ArticleId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; } = "percentage";
        public string? InstallationId { get; set; }
        public string? InstallationName { get; set; }

        public bool RequiresServiceOrder { get; set; }
        public bool ServiceOrderGenerated { get; set; }
        public string? ServiceOrderId { get; set; }
        public string? FulfillmentStatus { get; set; }
    }

    public class CreateSaleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ContactId { get; set; }
        public string Status { get; set; } = "won";
        public string? Stage { get; set; } = "closed";
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public string? Source { get; set; }
        public string Currency { get; set; } = "TND";
        public DateTime? EstimatedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public string? DeliveryCountry { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Discount { get; set; }
        public string? OfferId { get; set; }
        public List<CreateSaleItemDto>? Items { get; set; }
    }

    public class CreateSaleItemDto
    {
        public string Type { get; set; } = string.Empty;
        public string ArticleId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; } = 0;
        public string DiscountType { get; set; } = "percentage";
        public string? InstallationId { get; set; }
        public string? InstallationName { get; set; }

        public bool RequiresServiceOrder { get; set; } = false;
    }

    public class UpdateSaleDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Stage { get; set; }
        public string? Priority { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Discount { get; set; }
        public DateTime? EstimatedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public string? DeliveryCountry { get; set; }
        public string[]? Tags { get; set; }

        public string? LostReason { get; set; }
        public string? MaterialsFulfillment { get; set; }
        public string? ServiceOrdersStatus { get; set; }
    }

    public class SaleActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string SaleId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class SaleStatsDto
    {
        public long TotalSales { get; set; }
        public long WonSales { get; set; }
        public long LostSales { get; set; }
        public long ActiveSales { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageValue { get; set; }
        public decimal WinRate { get; set; }
        public decimal MonthlyGrowth { get; set; }

        public decimal ConversionRate { get; set; }
    }

    public class PaginatedSaleResponse
    {
        public List<SaleDto> Sales { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }
}
