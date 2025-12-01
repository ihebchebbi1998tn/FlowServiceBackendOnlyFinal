using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Modules.ServiceOrders.DTOs;
using MyApi.Modules.ServiceOrders.Models;
using MyApi.Modules.Contacts.Models;

namespace MyApi.Modules.ServiceOrders.Services
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceOrderService> _logger;

        public ServiceOrderService(ApplicationDbContext context, ILogger<ServiceOrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceOrderDto> CreateFromSaleAsync(string saleId, CreateServiceOrderDto createDto, string userId)
        {
            // Verify sale exists
            var sale = await _context.Sales.FindAsync(saleId);
            if (sale == null)
                throw new KeyNotFoundException($"Sale with ID {saleId} not found");

            // Check if service order already exists for this sale
            var existingOrder = await _context.ServiceOrders.FirstOrDefaultAsync(s => s.SaleId == saleId);
            if (existingOrder != null)
                throw new InvalidOperationException($"Service order already exists for sale {saleId}");

            var serviceOrderId = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";
            var orderNumber = $"SO-{DateTime.UtcNow:yyyy-MMdd}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";

            var serviceOrder = new ServiceOrder
            {
                Id = serviceOrderId,
                OrderNumber = orderNumber,
                SaleId = saleId,
                OfferId = sale.OfferId ?? string.Empty,
                ContactId = sale.ContactId,
                Status = "scheduled",
                Priority = createDto.Priority ?? "medium",
                Notes = createDto.Notes,
                StartDate = createDto.StartDate,
                TargetCompletionDate = createDto.TargetCompletionDate,
                EstimatedDuration = createDto.StartDate.HasValue && createDto.TargetCompletionDate.HasValue
                    ? (int)(createDto.TargetCompletionDate.Value - createDto.StartDate.Value).TotalHours
                    : null,
                EstimatedCost = sale.TotalAmount,
                ActualCost = 0,
                Discount = 0,
                DiscountPercentage = 0,
                Tax = 0,
                TotalAmount = sale.TotalAmount,
                PaymentStatus = "pending",
                PaymentTerms = "net30",
                CompletionPercentage = 0,
                RequiresApproval = createDto.RequiresApproval,
                Tags = createDto.Tags,
                CustomFields = createDto.CustomFields != null ? System.Text.Json.JsonSerializer.Serialize(createDto.CustomFields) : null,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ServiceOrders.Add(serviceOrder);

            // Create jobs from installations
            if (createDto.InstallationIds != null && createDto.InstallationIds.Any())
            {
                var jobs = createDto.InstallationIds.Select((instId, index) => new ServiceOrderJob
                {
                    Id = $"JOB-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    ServiceOrderId = serviceOrderId,
                    Title = $"Installation & Service - {instId}",
                    Status = "unscheduled",
                    InstallationId = instId,
                    WorkType = "maintenance",
                    EstimatedDuration = createDto.StartDate.HasValue && createDto.TargetCompletionDate.HasValue
                        ? (int)(createDto.TargetCompletionDate.Value - createDto.StartDate.Value).TotalHours / (createDto.InstallationIds.Length > 0 ? createDto.InstallationIds.Length : 1)
                        : null,
                    EstimatedCost = sale.TotalAmount / (createDto.InstallationIds.Length > 0 ? createDto.InstallationIds.Length : 1),
                    CompletionPercentage = 0,
                    AssignedTechnicianIds = createDto.AssignedTechnicianIds,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                _context.ServiceOrderJobs.AddRange(jobs);
            }

            await _context.SaveChangesAsync();

            var result = await GetServiceOrderByIdAsync(serviceOrderId);
            return result!;
        }

        public async Task<PaginatedServiceOrderResponse> GetServiceOrdersAsync(
            string? status = null,
            string? priority = null,
            int? contactId = null,
            string? saleId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? paymentStatus = null,
            string? search = null,
            int page = 1,
            int limit = 20,
            string sortBy = "created_at",
            string sortOrder = "desc"
        )
        {
            var query = _context.ServiceOrders.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(s => s.Status == status);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(s => s.Priority == priority);

            if (contactId.HasValue)
                query = query.Where(s => s.ContactId == contactId.Value);

            if (!string.IsNullOrEmpty(saleId))
                query = query.Where(s => s.SaleId == saleId);

            if (startDate.HasValue)
                query = query.Where(s => s.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.StartDate <= endDate.Value);

            if (!string.IsNullOrEmpty(paymentStatus))
                query = query.Where(s => s.PaymentStatus == paymentStatus);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s =>
                    s.OrderNumber.ToLower().Contains(searchLower) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchLower)) ||
                    (s.Notes != null && s.Notes.ToLower().Contains(searchLower))
                );
            }

            var total = await query.CountAsync();

            query = sortBy.ToLower() switch
            {
                "order_number" => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.OrderNumber) : query.OrderByDescending(s => s.OrderNumber),
                "start_date" => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.StartDate) : query.OrderByDescending(s => s.StartDate),
                "priority" => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.Priority) : query.OrderByDescending(s => s.Priority),
                "status" => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.Status) : query.OrderByDescending(s => s.Status),
                _ => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt)
            };

            var serviceOrders = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(s => s.Jobs)
                .ToListAsync();

            var contactIds = serviceOrders.Select(s => s.ContactId).Distinct().ToList();
            var contacts = await _context.Contacts
                .Where(c => contactIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);

            var dtos = serviceOrders.Select(s => MapToDto(s, contacts.GetValueOrDefault(s.ContactId))).ToList();

            return new PaginatedServiceOrderResponse
            {
                ServiceOrders = dtos,
                Pagination = new PaginationInfo
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPages = (int)Math.Ceiling((double)total / limit)
                }
            };
        }

        public async Task<ServiceOrderDto?> GetServiceOrderByIdAsync(string id, bool includeJobs = true)
        {
            var query = _context.ServiceOrders.AsQueryable();
            if (includeJobs)
                query = query.Include(s => s.Jobs);

            var serviceOrder = await query.FirstOrDefaultAsync(s => s.Id == id);
            if (serviceOrder == null)
                return null;

            var contact = await _context.Contacts.FindAsync(serviceOrder.ContactId);
            return MapToDto(serviceOrder, contact);
        }

        public async Task<ServiceOrderDto> UpdateServiceOrderAsync(string id, UpdateServiceOrderDto updateDto, string userId)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
                throw new KeyNotFoundException($"Service order with ID {id} not found");

            if (updateDto.Status != null) serviceOrder.Status = updateDto.Status;
            if (updateDto.Priority != null) serviceOrder.Priority = updateDto.Priority;
            if (updateDto.Description != null) serviceOrder.Description = updateDto.Description;
            if (updateDto.Notes != null) serviceOrder.Notes = updateDto.Notes;
            if (updateDto.StartDate.HasValue) serviceOrder.StartDate = updateDto.StartDate;
            if (updateDto.TargetCompletionDate.HasValue) serviceOrder.TargetCompletionDate = updateDto.TargetCompletionDate;
            if (updateDto.EstimatedDuration.HasValue) serviceOrder.EstimatedDuration = updateDto.EstimatedDuration;
            if (updateDto.Discount.HasValue) serviceOrder.Discount = updateDto.Discount;
            if (updateDto.DiscountPercentage.HasValue) serviceOrder.DiscountPercentage = updateDto.DiscountPercentage;
            if (updateDto.PaymentTerms != null) serviceOrder.PaymentTerms = updateDto.PaymentTerms;
            if (updateDto.RequiresApproval.HasValue) serviceOrder.RequiresApproval = updateDto.RequiresApproval.Value;
            if (updateDto.Tags != null) serviceOrder.Tags = updateDto.Tags;
            if (updateDto.CustomFields != null) serviceOrder.CustomFields = System.Text.Json.JsonSerializer.Serialize(updateDto.CustomFields);

            serviceOrder.UpdatedBy = userId;
            serviceOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = await GetServiceOrderByIdAsync(id);
            return result!;
        }

        public async Task<ServiceOrderDto> PatchServiceOrderAsync(string id, UpdateServiceOrderDto updateDto, string userId)
        {
            return await UpdateServiceOrderAsync(id, updateDto, userId);
        }

        public async Task<ServiceOrderDto> UpdateStatusAsync(string id, UpdateServiceOrderStatusDto statusDto, string userId)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
                throw new KeyNotFoundException($"Service order with ID {id} not found");

            // Validate status transition
            var validTransitions = GetValidStatusTransitions(serviceOrder.Status);
            if (!validTransitions.Contains(statusDto.Status))
                throw new InvalidOperationException($"Cannot transition from '{serviceOrder.Status}' to '{statusDto.Status}'");

            serviceOrder.Status = statusDto.Status;
            if (statusDto.Status == "in_progress" && !serviceOrder.ActualStartDate.HasValue)
                serviceOrder.ActualStartDate = DateTime.UtcNow;

            serviceOrder.UpdatedBy = userId;
            serviceOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = await GetServiceOrderByIdAsync(id);
            return result!;
        }

        public async Task<ServiceOrderDto> ApproveAsync(string id, ApproveServiceOrderDto approveDto, string userId)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
                throw new KeyNotFoundException($"Service order with ID {id} not found");

            if (!serviceOrder.RequiresApproval)
                throw new InvalidOperationException("Service order does not require approval");

            serviceOrder.ApprovedBy = userId;
            serviceOrder.ApprovalDate = approveDto.ApprovalDate ?? DateTime.UtcNow;
            serviceOrder.Status = "completed";
            serviceOrder.ActualCompletionDate = DateTime.UtcNow;
            serviceOrder.CompletionPercentage = 100;
            serviceOrder.UpdatedBy = userId;
            serviceOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = await GetServiceOrderByIdAsync(id);
            return result!;
        }

        public async Task<ServiceOrderDto> CompleteAsync(string id, CompleteServiceOrderDto completeDto, string userId)
        {
            var serviceOrder = await _context.ServiceOrders.Include(s => s.Jobs).FirstOrDefaultAsync(s => s.Id == id);
            if (serviceOrder == null)
                throw new KeyNotFoundException($"Service order with ID {id} not found");

            // Check if all jobs are completed
            if (serviceOrder.Jobs != null && serviceOrder.Jobs.Any(j => j.Status != "completed"))
                throw new InvalidOperationException("Not all jobs are completed");

            serviceOrder.Status = "completed";
            serviceOrder.ActualCompletionDate = DateTime.UtcNow;
            serviceOrder.CompletionPercentage = 100;
            serviceOrder.PaymentStatus = "pending";

            if (completeDto.GenerateInvoice)
            {
                serviceOrder.InvoiceNumber = $"INV-{DateTime.UtcNow:yyyy-MM-dd}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";
                serviceOrder.InvoiceDate = DateTime.UtcNow;
            }

            serviceOrder.UpdatedBy = userId;
            serviceOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = await GetServiceOrderByIdAsync(id);
            return result!;
        }

        public async Task<ServiceOrderDto> CancelAsync(string id, CancelServiceOrderDto cancelDto, string userId)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
                throw new KeyNotFoundException($"Service order with ID {id} not found");

            serviceOrder.Status = "cancelled";
            serviceOrder.CancellationReason = cancelDto.CancellationReason;
            serviceOrder.CancellationNotes = cancelDto.CancellationNotes;
            serviceOrder.UpdatedBy = userId;
            serviceOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = await GetServiceOrderByIdAsync(id);
            return result!;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
                return false;

            if (serviceOrder.Status != "draft")
                throw new InvalidOperationException("Cannot delete non-draft service order");

            _context.ServiceOrders.Remove(serviceOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceOrderStatsDto> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null, int? contactId = null)
        {
            var query = _context.ServiceOrders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(s => s.Status == status);

            if (contactId.HasValue)
                query = query.Where(s => s.ContactId == contactId.Value);

            var serviceOrders = await query.ToListAsync();

            var stats = new ServiceOrderStatsDto
            {
                TotalServiceOrders = serviceOrders.Count,
                ByStatus = new Dictionary<string, int>
                {
                    { "draft", serviceOrders.Count(s => s.Status == "draft") },
                    { "scheduled", serviceOrders.Count(s => s.Status == "scheduled") },
                    { "in_progress", serviceOrders.Count(s => s.Status == "in_progress") },
                    { "on_hold", serviceOrders.Count(s => s.Status == "on_hold") },
                    { "completed", serviceOrders.Count(s => s.Status == "completed") },
                    { "cancelled", serviceOrders.Count(s => s.Status == "cancelled") }
                },
                ByPriority = new Dictionary<string, int>
                {
                    { "low", serviceOrders.Count(s => s.Priority == "low") },
                    { "medium", serviceOrders.Count(s => s.Priority == "medium") },
                    { "high", serviceOrders.Count(s => s.Priority == "high") },
                    { "urgent", serviceOrders.Count(s => s.Priority == "urgent") }
                },
                Financials = new FinancialStatsDto
                {
                    TotalEstimatedCost = serviceOrders.Sum(s => s.EstimatedCost ?? 0),
                    TotalActualCost = serviceOrders.Sum(s => s.ActualCost ?? 0),
                    TotalDiscount = serviceOrders.Sum(s => s.Discount ?? 0),
                    TotalTax = serviceOrders.Sum(s => s.Tax ?? 0),
                    TotalBilled = serviceOrders.Sum(s => s.TotalAmount ?? 0),
                    TotalPaid = serviceOrders.Where(s => s.PaymentStatus == "paid").Sum(s => s.TotalAmount ?? 0),
                    TotalPending = serviceOrders.Where(s => s.PaymentStatus == "pending").Sum(s => s.TotalAmount ?? 0)
                },
                CompletionRate = serviceOrders.Count > 0 ? (double)serviceOrders.Count(s => s.Status == "completed") / serviceOrders.Count * 100 : 0,
                OnTimeCompletionRate = CalculateOnTimeCompletionRate(serviceOrders)
            };

            return stats;
        }

        private List<string> GetValidStatusTransitions(string currentStatus)
        {
            return currentStatus switch
            {
                "draft" => new List<string> { "scheduled", "cancelled" },
                "scheduled" => new List<string> { "in_progress", "on_hold", "cancelled" },
                "in_progress" => new List<string> { "on_hold", "completed", "cancelled" },
                "on_hold" => new List<string> { "in_progress", "cancelled" },
                "completed" => new List<string>(),
                "cancelled" => new List<string>(),
                _ => new List<string>()
            };
        }

        private double CalculateOnTimeCompletionRate(List<ServiceOrder> serviceOrders)
        {
            var completedOnTime = serviceOrders
                .Where(s => s.Status == "completed" && s.ActualCompletionDate.HasValue && s.TargetCompletionDate.HasValue && s.ActualCompletionDate <= s.TargetCompletionDate)
                .Count();

            var totalCompleted = serviceOrders.Count(s => s.Status == "completed");
            return totalCompleted > 0 ? (double)completedOnTime / totalCompleted * 100 : 0;
        }

        private ServiceOrderDto MapToDto(ServiceOrder serviceOrder, Contact? contact = null)
        {
            return new ServiceOrderDto
            {
                Id = serviceOrder.Id,
                OrderNumber = serviceOrder.OrderNumber,
                SaleId = serviceOrder.SaleId,
                OfferId = serviceOrder.OfferId,
                ContactId = serviceOrder.ContactId,
                Status = serviceOrder.Status,
                Priority = serviceOrder.Priority,
                Description = serviceOrder.Description,
                Notes = serviceOrder.Notes,
                StartDate = serviceOrder.StartDate,
                TargetCompletionDate = serviceOrder.TargetCompletionDate,
                ActualStartDate = serviceOrder.ActualStartDate,
                ActualCompletionDate = serviceOrder.ActualCompletionDate,
                EstimatedDuration = serviceOrder.EstimatedDuration,
                ActualDuration = serviceOrder.ActualDuration,
                EstimatedCost = serviceOrder.EstimatedCost,
                ActualCost = serviceOrder.ActualCost,
                Discount = serviceOrder.Discount,
                DiscountPercentage = serviceOrder.DiscountPercentage,
                Tax = serviceOrder.Tax,
                TotalAmount = serviceOrder.TotalAmount,
                PaymentStatus = serviceOrder.PaymentStatus,
                PaymentTerms = serviceOrder.PaymentTerms,
                InvoiceNumber = serviceOrder.InvoiceNumber,
                InvoiceDate = serviceOrder.InvoiceDate,
                CompletionPercentage = serviceOrder.CompletionPercentage,
                RequiresApproval = serviceOrder.RequiresApproval,
                ApprovedBy = serviceOrder.ApprovedBy,
                ApprovalDate = serviceOrder.ApprovalDate,
                Tags = serviceOrder.Tags,
                CustomFields = !string.IsNullOrEmpty(serviceOrder.CustomFields) 
                    ? System.Text.Json.JsonDocument.Parse(serviceOrder.CustomFields).RootElement 
                    : null,
                CreatedBy = serviceOrder.CreatedBy,
                CreatedAt = serviceOrder.CreatedAt,
                UpdatedBy = serviceOrder.UpdatedBy,
                UpdatedAt = serviceOrder.UpdatedAt,
                Jobs = serviceOrder.Jobs?.Select(j => new ServiceOrderJobDto
                {
                    Id = j.Id,
                    ServiceOrderId = j.ServiceOrderId,
                    Title = j.Title,
                    Description = j.Description,
                    Status = j.Status,
                    InstallationId = j.InstallationId,
                    WorkType = j.WorkType,
                    EstimatedDuration = j.EstimatedDuration,
                    EstimatedCost = j.EstimatedCost,
                    CompletionPercentage = j.CompletionPercentage,
                    AssignedTechnicianIds = j.AssignedTechnicianIds
                }).ToList(),
                Contact = contact != null ? new ContactSummaryDto
                {
                    Id = contact.Id,
                    Name = contact.Name,
                    Company = contact.Company,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    Address = contact.Address
                } : null
            };
        }
    }
}
