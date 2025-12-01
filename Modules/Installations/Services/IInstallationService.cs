using MyApi.Modules.Installations.DTOs;

namespace MyApi.Modules.Installations.Services
{
    public interface IInstallationService
    {
        Task<PaginatedInstallationResponse> GetInstallationsAsync(
            string? search = null,
            string? category = null,
            string? status = null,
            string? contactId = null,
            string? tags = null,
            bool? hasWarranty = null,
            string? maintenanceFrequency = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            int page = 1,
            int pageSize = 20,
            string sortBy = "created_at",
            string sortOrder = "desc"
        );

        Task<InstallationDto?> GetInstallationByIdAsync(string id);
        Task<InstallationDto> CreateInstallationAsync(CreateInstallationDto createDto, string userId);
        Task<InstallationDto> UpdateInstallationAsync(string id, UpdateInstallationDto updateDto, string userId);
        Task<bool> DeleteInstallationAsync(string id);
        Task<List<MaintenanceHistoryDto>> GetMaintenanceHistoryAsync(string installationId, int page = 1, int pageSize = 20);
        Task<MaintenanceHistoryDto> AddMaintenanceHistoryAsync(string installationId, MaintenanceHistoryDto historyDto, string userId);
    }
}
