using System.Collections.Generic;
using System.Threading.Tasks;
using MyApi.Modules.Dispatches.DTOs;

namespace MyApi.Modules.Dispatches.Services
{
    public interface IDispatchService
    {
        Task<DispatchDto> CreateFromJobAsync(string jobId, CreateDispatchFromJobDto dto, string userId);
        Task<PagedResult<DispatchListItemDto>> GetAllAsync(DispatchQueryParams query);
        Task<DispatchDto> GetByIdAsync(string dispatchId);
        Task<DispatchDto> UpdateAsync(string dispatchId, UpdateDispatchDto dto, string userId);
        Task<DispatchDto> UpdateStatusAsync(string dispatchId, UpdateDispatchStatusDto dto, string userId);
        Task<DispatchDto> StartDispatchAsync(string dispatchId, StartDispatchDto dto, string userId);
        Task<DispatchDto> CompleteDispatchAsync(string dispatchId, CompleteDispatchDto dto, string userId);
        Task DeleteAsync(string dispatchId, string userId);

        // Time entries
        Task<TimeEntryDto> AddTimeEntryAsync(string dispatchId, CreateTimeEntryDto dto, string userId);
        Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(string dispatchId);
        Task ApproveTimeEntryAsync(string dispatchId, string timeEntryId, ApproveTimeEntryDto dto, string userId);

        // Expenses
        Task<ExpenseDto> AddExpenseAsync(string dispatchId, CreateExpenseDto dto, string userId);
        Task<IEnumerable<ExpenseDto>> GetExpensesAsync(string dispatchId);
        Task ApproveExpenseAsync(string dispatchId, string expenseId, ApproveExpenseDto dto, string userId);

        // Materials
        Task<MaterialDto> AddMaterialUsageAsync(string dispatchId, CreateMaterialUsageDto dto, string userId);
        Task<IEnumerable<MaterialDto>> GetMaterialsAsync(string dispatchId);
        Task ApproveMaterialAsync(string dispatchId, string materialId, ApproveMaterialDto dto, string userId);

        // Attachments & Notes
    Task<AttachmentUploadResponseDto> UploadAttachmentAsync(string dispatchId, Microsoft.AspNetCore.Http.IFormFile file, string category, string? description, double? latitude, double? longitude, string userId);
        Task<NoteDto> AddNoteAsync(string dispatchId, CreateNoteDto dto, string userId);

        // Statistics
        Task<DispatchStatisticsDto> GetStatisticsAsync(StatisticsQueryParams query);
    }
}
