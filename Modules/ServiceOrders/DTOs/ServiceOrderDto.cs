namespace MyApi.Modules.ServiceOrders.DTOs
{
    public class ServiceOrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string SaleId { get; set; } = string.Empty;
        public string OfferId { get; set; } = string.Empty;
        public int ContactId { get; set; }
        public string Status { get; set; } = "draft";
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public int? EstimatedDuration { get; set; }
        public int? ActualDuration { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? Discount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? Tax { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentTerms { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? CompletionPercentage { get; set; }
        public bool RequiresApproval { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string[]? Tags { get; set; }
        public object? CustomFields { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ServiceOrderJobDto>? Jobs { get; set; }
        public ContactSummaryDto? Contact { get; set; }
    }

    public class ServiceOrderJobDto
    {
        public string Id { get; set; } = string.Empty;
        public string ServiceOrderId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "unscheduled";
        public string? InstallationId { get; set; }
        public string? WorkType { get; set; }
        public int? EstimatedDuration { get; set; }
        public decimal? EstimatedCost { get; set; }
        public int? CompletionPercentage { get; set; }
        public string[]? AssignedTechnicianIds { get; set; }
    }

    public class CreateServiceOrderDto
    {
        public string[]? InstallationIds { get; set; }
        public string[]? AssignedTechnicianIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public string? Priority { get; set; }
        public string? Notes { get; set; }
        public bool RequiresApproval { get; set; }
        public string[]? Tags { get; set; }
        public object? CustomFields { get; set; }
    }

    public class UpdateServiceOrderDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public int? EstimatedDuration { get; set; }
        public decimal? Discount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? PaymentTerms { get; set; }
        public bool? RequiresApproval { get; set; }
        public string[]? Tags { get; set; }
        public object? CustomFields { get; set; }
    }

    public class UpdateServiceOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class ApproveServiceOrderDto
    {
        public string? ApprovalNotes { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }

    public class CompleteServiceOrderDto
    {
        public string? CompletionNotes { get; set; }
        public bool GenerateInvoice { get; set; }
        public string? InvoiceNotes { get; set; }
    }

    public class CancelServiceOrderDto
    {
        public string? CancellationReason { get; set; }
        public string? CancellationNotes { get; set; }
    }

    public class ServiceOrderStatsDto
    {
        public int TotalServiceOrders { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public Dictionary<string, int> ByPriority { get; set; } = new();
        public FinancialStatsDto Financials { get; set; } = new();
        public double AverageCompletionTime { get; set; }
        public double CompletionRate { get; set; }
        public double OnTimeCompletionRate { get; set; }
    }

    public class FinancialStatsDto
    {
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
    }

    public class PaginatedServiceOrderResponse
    {
        public List<ServiceOrderDto> ServiceOrders { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
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
}
