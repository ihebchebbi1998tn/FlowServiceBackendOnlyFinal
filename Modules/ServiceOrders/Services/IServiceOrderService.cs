using MyApi.Modules.ServiceOrders.DTOs;

namespace MyApi.Modules.ServiceOrders.Services
{
    public interface IServiceOrderService
    {
        Task<ServiceOrderDto> CreateFromSaleAsync(string saleId, CreateServiceOrderDto createDto, string userId);
        Task<PaginatedServiceOrderResponse> GetServiceOrdersAsync(
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
        );
        Task<ServiceOrderDto?> GetServiceOrderByIdAsync(string id, bool includeJobs = true);
        Task<ServiceOrderDto> UpdateServiceOrderAsync(string id, UpdateServiceOrderDto updateDto, string userId);
        Task<ServiceOrderDto> PatchServiceOrderAsync(string id, UpdateServiceOrderDto updateDto, string userId);
        Task<ServiceOrderDto> UpdateStatusAsync(string id, UpdateServiceOrderStatusDto statusDto, string userId);
        Task<ServiceOrderDto> ApproveAsync(string id, ApproveServiceOrderDto approveDto, string userId);
        Task<ServiceOrderDto> CompleteAsync(string id, CompleteServiceOrderDto completeDto, string userId);
        Task<ServiceOrderDto> CancelAsync(string id, CancelServiceOrderDto cancelDto, string userId);
        Task<bool> DeleteAsync(string id);
        Task<ServiceOrderStatsDto> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null, int? contactId = null);
    }
}
