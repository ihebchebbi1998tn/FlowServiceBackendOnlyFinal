using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Modules.Sales.DTOs;
using MyApi.Modules.Sales.Models;
using MyApi.Modules.Contacts.Models;
using MyApi.Modules.Offers.Models;

namespace MyApi.Modules.Sales.Services
{
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;

        public SaleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedSaleResponse> GetSalesAsync(
            string? status = null,
            string? stage = null,
            string? priority = null,
            string? contactId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string? search = null,
            int page = 1,
            int limit = 20,
            string sortBy = "updated_at",
            string sortOrder = "desc"
        )
        {
            var query = _context.Sales.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(s => s.Status == status);

            if (!string.IsNullOrEmpty(stage))
                query = query.Where(s => s.Stage == stage);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(s => s.Priority == priority);

            if (!string.IsNullOrEmpty(contactId) && int.TryParse(contactId, out int contactIdInt))
                query = query.Where(s => s.ContactId == contactIdInt);

            if (dateFrom.HasValue)
                query = query.Where(s => s.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(s => s.CreatedAt <= dateTo.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s =>
                    s.Title.ToLower().Contains(searchLower) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchLower))
                );
            }

            // Count total
            var total = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "created_at" => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                "title" => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.Title) : query.OrderByDescending(s => s.Title),
                "amount" => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.Amount) : query.OrderByDescending(s => s.Amount),
                _ => sortOrder.ToLower() == "asc" ? query.OrderBy(s => s.UpdatedAt) : query.OrderByDescending(s => s.UpdatedAt)
            };

            // Apply pagination
            var sales = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(s => s.Items)
                .ToListAsync();
                
            // Get contact IDs
            var contactIds = sales.Select(s => s.ContactId).Distinct().ToList();
            var contacts = await _context.Contacts
                .Where(c => contactIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);

            var saleDtos = sales.Select(s => MapToDto(s, contacts.GetValueOrDefault(s.ContactId))).ToList();

            return new PaginatedSaleResponse
            {
                Sales = saleDtos,
                Pagination = new PaginationInfo
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPages = (int)Math.Ceiling((double)total / limit)
                }
            };
        }

        public async Task<SaleDto?> GetSaleByIdAsync(string id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return null;
            
            var contact = await _context.Contacts.FindAsync(sale.ContactId);
            return MapToDto(sale, contact);
        }

        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto createDto, string userId)
        {
            var saleId = $"SALE-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Verify contact exists
            var contact = await _context.Contacts.FindAsync(createDto.ContactId);
            if (contact == null)
                throw new KeyNotFoundException($"Contact with ID {createDto.ContactId} not found");

            var sale = new Sale
            {
                Id = saleId,
                Title = createDto.Title,
                Description = createDto.Description,
                ContactId = createDto.ContactId,
                Status = createDto.Status,
                Stage = createDto.Stage,
                Priority = createDto.Priority,
                Currency = createDto.Currency,
                EstimatedCloseDate = createDto.EstimatedCloseDate,
                BillingAddress = createDto.BillingAddress,
                BillingPostalCode = createDto.BillingPostalCode,
                BillingCountry = createDto.BillingCountry,
                DeliveryAddress = createDto.DeliveryAddress,
                DeliveryPostalCode = createDto.DeliveryPostalCode,
                DeliveryCountry = createDto.DeliveryCountry,
                Taxes = createDto.Taxes ?? 0,
                Discount = createDto.Discount ?? 0,
                Amount = 0, // Will be calculated from items
                OfferId = createDto.OfferId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tags = new string[] { }
            };

            _context.Sales.Add(sale);

            // Add items if provided
            if (createDto.Items != null && createDto.Items.Any())
            {
                var items = createDto.Items.Select(itemDto => new SaleItem
                {
                    Id = $"SALEITEM-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    SaleId = saleId,
                    Type = itemDto.Type,
                    ArticleId = itemDto.ArticleId,
                    ItemName = itemDto.ItemName,
                    ItemCode = itemDto.ItemCode,
                    Description = itemDto.Description,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    Discount = itemDto.Discount,
                    DiscountType = itemDto.DiscountType,
                    InstallationId = itemDto.InstallationId,
                    InstallationName = itemDto.InstallationName,
                    RequiresServiceOrder = itemDto.RequiresServiceOrder
                }).ToList();

                _context.SaleItems.AddRange(items);
            }

            await _context.SaveChangesAsync();

            // Reload to get computed values
            var createdSale = await GetSaleByIdAsync(saleId);
            return createdSale!;
        }

        public async Task<SaleDto> CreateSaleFromOfferAsync(string offerId, string userId)
        {
            // Get the offer with all its items
            var offer = await _context.Offers
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                throw new KeyNotFoundException($"Offer with ID {offerId} not found");

            var saleId = $"SALE-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Create sale as exact copy of offer
            var sale = new Sale
            {
                Id = saleId,
                Title = offer.Title,
                Description = offer.Description,
                ContactId = offer.ContactId,
                Status = "won", // Default to won for converted offers
                Stage = "closed",
                Priority = "medium",
                Currency = offer.Currency,
                BillingAddress = offer.BillingAddress,
                BillingPostalCode = offer.BillingPostalCode,
                BillingCountry = offer.BillingCountry,
                DeliveryAddress = offer.DeliveryAddress,
                DeliveryPostalCode = offer.DeliveryPostalCode,
                DeliveryCountry = offer.DeliveryCountry,
                Taxes = offer.Taxes,
                Discount = offer.Discount,
                Amount = offer.Amount,
                AssignedTo = offer.AssignedTo,
                AssignedToName = offer.AssignedToName,
                Tags = offer.Tags != null ? offer.Tags.Concat(new[] { "Converted" }).ToArray() : new[] { "Converted" },
                OfferId = offer.Id,
                ConvertedFromOfferAt = DateTime.UtcNow,
                ActualCloseDate = DateTime.UtcNow,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Sales.Add(sale);

            // Copy all items from offer
            if (offer.Items != null && offer.Items.Any())
            {
                var saleItems = offer.Items.Select(offerItem => new SaleItem
                {
                    Id = $"SALEITEM-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    SaleId = saleId,
                    Type = offerItem.Type,
                    ArticleId = offerItem.ArticleId,
                    ItemName = offerItem.ItemName,
                    ItemCode = offerItem.ItemCode,
                    Description = offerItem.Description,
                    Quantity = offerItem.Quantity,
                    UnitPrice = offerItem.UnitPrice,
                    Discount = offerItem.Discount,
                    DiscountType = offerItem.DiscountType,
                    InstallationId = offerItem.InstallationId,
                    InstallationName = offerItem.InstallationName,
                    RequiresServiceOrder = offerItem.Type == "service",
                    FulfillmentStatus = "pending"
                }).ToList();

                _context.SaleItems.AddRange(saleItems);
            }

            // Update the offer to mark it as converted
            offer.Status = "accepted";
            offer.ConvertedToSaleId = saleId;
            offer.ConvertedAt = DateTime.UtcNow;
            offer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload to get computed values
            var createdSale = await GetSaleByIdAsync(saleId);
            return createdSale!;
        }

        public async Task<SaleDto> UpdateSaleAsync(string id, UpdateSaleDto updateDto, string userId)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
                throw new KeyNotFoundException($"Sale with ID {id} not found");

            // Update only provided fields
            if (updateDto.Title != null) sale.Title = updateDto.Title;
            if (updateDto.Description != null) sale.Description = updateDto.Description;
            if (updateDto.Status != null) sale.Status = updateDto.Status;
            if (updateDto.Stage != null) sale.Stage = updateDto.Stage;
            if (updateDto.Priority != null) sale.Priority = updateDto.Priority;
            if (updateDto.Amount.HasValue) sale.Amount = updateDto.Amount.Value;
            if (updateDto.Taxes.HasValue) sale.Taxes = updateDto.Taxes.Value;
            if (updateDto.Discount.HasValue) sale.Discount = updateDto.Discount.Value;
            if (updateDto.EstimatedCloseDate.HasValue) sale.EstimatedCloseDate = updateDto.EstimatedCloseDate;
            if (updateDto.ActualCloseDate.HasValue) sale.ActualCloseDate = updateDto.ActualCloseDate;
            if (updateDto.BillingAddress != null) sale.BillingAddress = updateDto.BillingAddress;
            if (updateDto.BillingPostalCode != null) sale.BillingPostalCode = updateDto.BillingPostalCode;
            if (updateDto.BillingCountry != null) sale.BillingCountry = updateDto.BillingCountry;
            if (updateDto.DeliveryAddress != null) sale.DeliveryAddress = updateDto.DeliveryAddress;
            if (updateDto.DeliveryPostalCode != null) sale.DeliveryPostalCode = updateDto.DeliveryPostalCode;
            if (updateDto.DeliveryCountry != null) sale.DeliveryCountry = updateDto.DeliveryCountry;
            if (updateDto.LostReason != null) sale.LostReason = updateDto.LostReason;
            if (updateDto.MaterialsFulfillment != null) sale.MaterialsFulfillment = updateDto.MaterialsFulfillment;
            if (updateDto.ServiceOrdersStatus != null) sale.ServiceOrdersStatus = updateDto.ServiceOrdersStatus;
            if (updateDto.Tags != null) sale.Tags = updateDto.Tags;

            sale.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedSale = await GetSaleByIdAsync(id);
            return updatedSale!;
        }

        public async Task<bool> DeleteSaleAsync(string id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
                return false;

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SaleStatsDto> GetSaleStatsAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.Sales.AsQueryable();

            if (dateFrom.HasValue)
                query = query.Where(s => s.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(s => s.CreatedAt <= dateTo.Value);

            var sales = await query.ToListAsync();

            var totalSales = sales.Count;
            var activeSales = sales.Count(s => new[] { "new_offer", "draft", "sent", "accepted" }.Contains(s.Status));
            var wonSales = sales.Count(s => new[] { "won", "completed" }.Contains(s.Status));
            var lostSales = sales.Count(s => new[] { "lost", "cancelled" }.Contains(s.Status));
            var totalValue = sales.Sum(s => s.TotalAmount ?? s.Amount);
            var averageValue = totalSales > 0 ? totalValue / totalSales : 0;
            var conversionRate = totalSales > 0 ? (decimal)wonSales / totalSales * 100 : 0;

            return new SaleStatsDto
            {
                TotalSales = totalSales,
                ActiveSales = activeSales,
                WonSales = wonSales,
                LostSales = lostSales,
                TotalValue = totalValue,
                AverageValue = averageValue,
                ConversionRate = Math.Round(conversionRate, 2),
                MonthlyGrowth = 15.2m // Mock value - implement proper calculation
            };
        }

        public async Task<SaleItemDto> AddSaleItemAsync(string saleId, CreateSaleItemDto itemDto)
        {
            var sale = await _context.Sales.FindAsync(saleId);
            if (sale == null)
                throw new KeyNotFoundException($"Sale with ID {saleId} not found");

            var item = new SaleItem
            {
                Id = $"SALEITEM-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                SaleId = saleId,
                Type = itemDto.Type,
                ArticleId = itemDto.ArticleId,
                ItemName = itemDto.ItemName,
                ItemCode = itemDto.ItemCode,
                Description = itemDto.Description,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Discount = itemDto.Discount,
                DiscountType = itemDto.DiscountType,
                InstallationId = itemDto.InstallationId,
                InstallationName = itemDto.InstallationName,
                RequiresServiceOrder = itemDto.RequiresServiceOrder
            };

            _context.SaleItems.Add(item);
            await _context.SaveChangesAsync();

            // Reload to get computed total_price
            var addedItem = await _context.SaleItems.FindAsync(item.Id);
            return MapItemToDto(addedItem!);
        }

        public async Task<SaleItemDto> UpdateSaleItemAsync(string saleId, string itemId, CreateSaleItemDto itemDto)
        {
            var item = await _context.SaleItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.SaleId == saleId);

            if (item == null)
                throw new KeyNotFoundException($"Item with ID {itemId} not found in sale {saleId}");

            item.Type = itemDto.Type;
            item.ArticleId = itemDto.ArticleId;
            item.ItemName = itemDto.ItemName;
            item.ItemCode = itemDto.ItemCode;
            item.Description = itemDto.Description;
            item.Quantity = itemDto.Quantity;
            item.UnitPrice = itemDto.UnitPrice;
            item.Discount = itemDto.Discount;
            item.DiscountType = itemDto.DiscountType;
            item.InstallationId = itemDto.InstallationId;
            item.InstallationName = itemDto.InstallationName;
            item.RequiresServiceOrder = itemDto.RequiresServiceOrder;

            await _context.SaveChangesAsync();

            // Reload to get computed values
            var updatedItem = await _context.SaleItems.FindAsync(itemId);
            return MapItemToDto(updatedItem!);
        }

        public async Task<bool> DeleteSaleItemAsync(string saleId, string itemId)
        {
            var item = await _context.SaleItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.SaleId == saleId);

            if (item == null)
                return false;

            _context.SaleItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SaleActivityDto>> GetSaleActivitiesAsync(string saleId, string? type = null, int page = 1, int limit = 20)
        {
            var query = _context.SaleActivities
                .Where(a => a.SaleId == saleId);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(a => a.Type == type);

            var activities = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return activities.Select(a => new SaleActivityDto
            {
                Id = a.Id,
                SaleId = a.SaleId,
                Type = a.Type,
                Description = a.Description,
                Details = a.Details,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                CreatedByName = a.CreatedByName
            }).ToList();
        }

        private SaleDto MapToDto(Sale sale, Contact? contact = null)
        {
            return new SaleDto
            {
                Id = sale.Id,
                Title = sale.Title,
                Description = sale.Description,
                ContactId = sale.ContactId,
                Contact = contact != null ? new ContactSummaryDto
                {
                    Id = contact.Id,
                    Name = contact.Name,
                    Company = contact.Company,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    Address = contact.Address
                } : null,
                Amount = sale.Amount,
                Currency = sale.Currency,
                Taxes = sale.Taxes,
                Discount = sale.Discount,
                TotalAmount = sale.TotalAmount,
                Status = sale.Status,
                Stage = sale.Stage,
                Priority = sale.Priority,
                BillingAddress = sale.BillingAddress,
                BillingPostalCode = sale.BillingPostalCode,
                BillingCountry = sale.BillingCountry,
                DeliveryAddress = sale.DeliveryAddress,
                DeliveryPostalCode = sale.DeliveryPostalCode,
                DeliveryCountry = sale.DeliveryCountry,
                EstimatedCloseDate = sale.EstimatedCloseDate,
                ActualCloseDate = sale.ActualCloseDate,
                AssignedTo = sale.AssignedTo,
                AssignedToName = sale.AssignedToName,
                Tags = sale.Tags,
                Items = sale.Items?.Select(MapItemToDto).ToList(),
                CreatedAt = sale.CreatedAt,
                UpdatedAt = sale.UpdatedAt,
                CreatedBy = sale.CreatedBy,
                LastActivity = sale.LastActivity,
                OfferId = sale.OfferId,
                ConvertedFromOfferAt = sale.ConvertedFromOfferAt,
                LostReason = sale.LostReason,
                MaterialsFulfillment = sale.MaterialsFulfillment,
                ServiceOrdersStatus = sale.ServiceOrdersStatus
            };
        }

        private SaleItemDto MapItemToDto(SaleItem item)
        {
            return new SaleItemDto
            {
                Id = item.Id,
                SaleId = item.SaleId,
                Type = item.Type,
                ArticleId = item.ArticleId,
                ItemName = item.ItemName,
                ItemCode = item.ItemCode,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice ?? 0,
                Discount = item.Discount,
                DiscountType = item.DiscountType,
                InstallationId = item.InstallationId,
                InstallationName = item.InstallationName,
                RequiresServiceOrder = item.RequiresServiceOrder,
                ServiceOrderGenerated = item.ServiceOrderGenerated,
                ServiceOrderId = item.ServiceOrderId,
                FulfillmentStatus = item.FulfillmentStatus
            };
        }
    }
}
