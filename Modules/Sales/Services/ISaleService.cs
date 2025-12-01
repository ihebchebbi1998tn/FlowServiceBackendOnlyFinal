using MyApi.Modules.Sales.DTOs;

namespace MyApi.Modules.Sales.Services
{
    public interface ISaleService
    {
        Task<PaginatedSaleResponse> GetSalesAsync(
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
        );
        
        Task<SaleDto?> GetSaleByIdAsync(string id);
        Task<SaleDto> CreateSaleAsync(CreateSaleDto createDto, string userId);
        Task<SaleDto> CreateSaleFromOfferAsync(string offerId, string userId);
        Task<SaleDto> UpdateSaleAsync(string id, UpdateSaleDto updateDto, string userId);
        Task<bool> DeleteSaleAsync(string id);
        Task<SaleStatsDto> GetSaleStatsAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
        
        // Sale Items
        Task<SaleItemDto> AddSaleItemAsync(string saleId, CreateSaleItemDto itemDto);
        Task<SaleItemDto> UpdateSaleItemAsync(string saleId, string itemId, CreateSaleItemDto itemDto);
        Task<bool> DeleteSaleItemAsync(string saleId, string itemId);
        
        // Sale Activities
        Task<List<SaleActivityDto>> GetSaleActivitiesAsync(string saleId, string? type = null, int page = 1, int limit = 20);
    }
}
