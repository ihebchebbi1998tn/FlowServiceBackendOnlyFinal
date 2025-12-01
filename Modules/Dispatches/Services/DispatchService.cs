using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApi.Data;
using MyApi.Modules.Dispatches.DTOs;
using MyApi.Modules.Dispatches.Models;
using MyApi.Modules.Dispatches.Mapping;

namespace MyApi.Modules.Dispatches.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DispatchService> _logger;

        public DispatchService(ApplicationDbContext db, ILogger<DispatchService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<DispatchDto> CreateFromJobAsync(string jobId, CreateDispatchFromJobDto dto, string userId)
        {
            var id = Guid.NewGuid().ToString();
            var dispatch = new Dispatch
            {
                Id = id,
                DispatchNumber = $"DISP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                JobId = jobId,
                ServiceOrderId = null,
                Status = "pending",
                Priority = dto.Priority ?? "medium",
                ScheduledDate = dto.ScheduledDate,
                ScheduledStartTime = dto.ScheduledStartTime,
                ScheduledEndTime = dto.ScheduledEndTime,
                EstimatedDuration = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DispatchedBy = userId,
                DispatchedAt = DateTime.UtcNow
            };

            // Assigned technicians
            foreach (var techId in dto.AssignedTechnicianIds ?? Enumerable.Empty<string>())
            {
                var dt = new DispatchTechnician
                {
                    DispatchId = id,
                    TechnicianId = techId,
                    AssignedAt = DateTime.UtcNow
                };
                dispatch.AssignedTechnicians.Add(dt);
                _db.DispatchTechnicians.Add(dt);
            }

            _db.Dispatches.Add(dispatch);
            await _db.SaveChangesAsync();

            return DispatchMapping.ToDto(dispatch);
        }

        public async Task<PagedResult<DispatchListItemDto>> GetAllAsync(DispatchQueryParams query)
        {
            var q = _db.Dispatches.AsQueryable().Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(query.Status)) q = q.Where(d => d.Status == query.Status);
            if (!string.IsNullOrEmpty(query.Priority)) q = q.Where(d => d.Priority == query.Priority);
            if (!string.IsNullOrEmpty(query.TechnicianId)) q = q.Where(d => d.AssignedTechnicians.Any(at => at.TechnicianId == query.TechnicianId));
            if (!string.IsNullOrEmpty(query.ServiceOrderId)) q = q.Where(d => d.ServiceOrderId == query.ServiceOrderId);
            if (query.DateFrom.HasValue) q = q.Where(d => d.ScheduledDate >= query.DateFrom.Value);
            if (query.DateTo.HasValue) q = q.Where(d => d.ScheduledDate <= query.DateTo.Value);

            var total = await q.CountAsync();

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Min(100, Math.Max(1, query.PageSize));

            var items = await q
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DispatchListItemDto
                {
                    Id = d.Id,
                    DispatchNumber = d.DispatchNumber,
                    JobId = d.JobId,
                    ServiceOrderId = d.ServiceOrderId,
                    Status = d.Status,
                    Priority = d.Priority,
                    AssignedTechnicians = d.AssignedTechnicians.Select(at => new UserLightDto { Id = at.TechnicianId, Name = at.Name, Email = at.Email }).ToList(),
                    Scheduling = new SchedulingDto { ScheduledDate = d.ScheduledDate ?? default, ScheduledStartTime = d.ScheduledStartTime, ScheduledEndTime = d.ScheduledEndTime, EstimatedDuration = d.EstimatedDuration }
                })
                .ToListAsync();

            return new PagedResult<DispatchListItemDto>
            {
                Data = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<DispatchDto> GetByIdAsync(string dispatchId)
        {
            var d = await _db.Dispatches
                .Include(x => x.TimeEntries)
                .Include(x => x.Expenses)
                .Include(x => x.MaterialsUsed)
                .Include(x => x.Attachments)
                .Include(x => x.Notes)
                .Include(x => x.AssignedTechnicians)
                .FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);

            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            var dto = DispatchMapping.ToDto(d);
            // populate nested lists minimally
            dto.TimeEntries = d.TimeEntries.Select(t => new { t.Id, t.Duration, t.Status }).Cast<object>().ToList();
            dto.Expenses = d.Expenses.Select(e => new { e.Id, e.Amount, e.Status }).Cast<object>().ToList();
            dto.MaterialsUsed = d.MaterialsUsed.Select(m => new { m.Id, m.ArticleName, m.Quantity, m.TotalPrice }).Cast<object>().ToList();
            dto.Attachments = d.Attachments.Select(a => new { a.Id, a.FileName, a.Category }).Cast<object>().ToList();
            dto.Notes = d.Notes.Select(n => new { n.Id, n.Content, n.CreatedAt }).Cast<object>().ToList();

            return dto;
        }

        // The rest of methods are not implemented yet but are required by the interface
        public async Task<DispatchDto> UpdateAsync(string dispatchId, UpdateDispatchDto dto, string userId)
        {
            var d = await _db.Dispatches.Include(x => x.AssignedTechnicians).FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            if (dto.AssignedTechnicianIds != null)
            {
                // Replace assigned technicians
                var existing = d.AssignedTechnicians.ToList();
                foreach (var ex in existing) _db.DispatchTechnicians.Remove(ex);
                d.AssignedTechnicians.Clear();
                foreach (var tech in dto.AssignedTechnicianIds)
                {
                    var dt = new DispatchTechnician { DispatchId = d.Id, TechnicianId = tech, AssignedAt = DateTime.UtcNow };
                    d.AssignedTechnicians.Add(dt);
                    _db.DispatchTechnicians.Add(dt);
                }
            }

            if (dto.ScheduledDate.HasValue) d.ScheduledDate = dto.ScheduledDate.Value;
            if (dto.ScheduledStartTime.HasValue) d.ScheduledStartTime = dto.ScheduledStartTime.Value;
            if (dto.ScheduledEndTime.HasValue) d.ScheduledEndTime = dto.ScheduledEndTime.Value;
            if (!string.IsNullOrEmpty(dto.Priority)) d.Priority = dto.Priority;

            d.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return DispatchMapping.ToDto(d);
        }

        public async Task<DispatchDto> UpdateStatusAsync(string dispatchId, UpdateDispatchStatusDto dto, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            var validTransitions = new Dictionary<string, List<string>>
            {
                { "pending", new List<string>{ "assigned", "cancelled" } },
                { "assigned", new List<string>{ "in_progress", "cancelled" } },
                { "in_progress", new List<string>{ "completed", "cancelled" } },
                { "completed", new List<string>() },
                { "cancelled", new List<string>() }
            };

            var current = d.Status;
            var requested = dto.Status;
            if (current == requested) return DispatchMapping.ToDto(d);
            if (!validTransitions.ContainsKey(current) || !validTransitions[current].Contains(requested))
            {
                throw new ArgumentException("Invalid status transition", nameof(dto.Status));
            }

            d.Status = requested;
            d.UpdatedAt = DateTime.UtcNow;
            if (requested == "assigned") { /* nothing special */ }
            if (requested == "in_progress") d.ActualStartTime = DateTime.UtcNow;
            if (requested == "completed") d.ActualEndTime = DateTime.UtcNow;
            if (requested == "cancelled") { /* set cancel metadata if needed */ }

            await _db.SaveChangesAsync();
            return DispatchMapping.ToDto(d);
        }

        public async Task<DispatchDto> StartDispatchAsync(string dispatchId, StartDispatchDto dto, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");
            if (d.Status != "assigned" && d.Status != "pending") throw new ArgumentException("Dispatch must be assigned or pending to start");

            d.Status = "in_progress";
            d.ActualStartTime = dto.ActualStartTime;
            d.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return DispatchMapping.ToDto(d);
        }

        public async Task<DispatchDto> CompleteDispatchAsync(string dispatchId, CompleteDispatchDto dto, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");
            if (d.Status != "in_progress") throw new ArgumentException("Dispatch must be in_progress to complete");

            d.Status = "completed";
            d.ActualEndTime = dto.ActualEndTime;
            d.ActualDuration = dto.CompletionPercentage > 0 ? dto.CompletionPercentage : d.ActualDuration;
            d.CompletionPercentage = dto.CompletionPercentage;
            d.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return DispatchMapping.ToDto(d);
        }

        public async Task DeleteAsync(string dispatchId, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) return;
            if (d.Status != "pending" && d.Status != "cancelled") throw new InvalidOperationException("Cannot delete dispatch unless pending or cancelled");
            d.IsDeleted = true;
            d.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<TimeEntryDto> AddTimeEntryAsync(string dispatchId, CreateTimeEntryDto dto, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            var te = new TimeEntry
            {
                Id = Guid.NewGuid().ToString(),
                DispatchId = dispatchId,
                TechnicianId = dto.TechnicianId,
                WorkType = dto.WorkType,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Duration = (int)(dto.EndTime - dto.StartTime).TotalMinutes,
                Description = dto.Description,
                Billable = dto.Billable,
                HourlyRate = dto.HourlyRate,
                TotalCost = dto.HourlyRate.HasValue ? Math.Round(dto.HourlyRate.Value * (decimal)((dto.EndTime - dto.StartTime).TotalHours), 2) : null,
                Status = "completed",
                CreatedAt = DateTime.UtcNow
            };
            _db.TimeEntries.Add(te);
            await _db.SaveChangesAsync();

            return new TimeEntryDto { Id = te.Id, DispatchId = te.DispatchId, TechnicianId = te.TechnicianId, WorkType = te.WorkType, Duration = te.Duration, TotalCost = te.TotalCost, Status = te.Status, CreatedAt = te.CreatedAt };
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(string dispatchId)
        {
            var items = await _db.TimeEntries.Where(t => t.DispatchId == dispatchId).ToListAsync();
            return items.Select(t => new TimeEntryDto { Id = t.Id, DispatchId = t.DispatchId, TechnicianId = t.TechnicianId, WorkType = t.WorkType, Duration = t.Duration, TotalCost = t.TotalCost, Status = t.Status, CreatedAt = t.CreatedAt }).ToList();
        }

        public async Task ApproveTimeEntryAsync(string dispatchId, string timeEntryId, ApproveTimeEntryDto dto, string userId)
        {
            var te = await _db.TimeEntries.FirstOrDefaultAsync(t => t.Id == timeEntryId && t.DispatchId == dispatchId);
            if (te == null) throw new KeyNotFoundException("Time entry not found");
            te.Status = "approved";
            te.ApprovedBy = dto.ApprovedBy;
            te.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<ExpenseDto> AddExpenseAsync(string dispatchId, CreateExpenseDto dto, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            var exp = new Expense
            {
                Id = Guid.NewGuid().ToString(),
                DispatchId = dispatchId,
                TechnicianId = dto.TechnicianId,
                Type = dto.Type,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Description = dto.Description,
                Date = dto.Date ?? DateTime.UtcNow,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            _db.DispatchExpenses.Add(exp);
            await _db.SaveChangesAsync();

            return new ExpenseDto { Id = exp.Id, DispatchId = exp.DispatchId, TechnicianId = exp.TechnicianId, Type = exp.Type, Amount = exp.Amount, Status = exp.Status, CreatedAt = exp.CreatedAt };
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(string dispatchId)
        {
            var items = await _db.DispatchExpenses.Where(e => e.DispatchId == dispatchId).ToListAsync();
            return items.Select(e => new ExpenseDto { Id = e.Id, DispatchId = e.DispatchId, TechnicianId = e.TechnicianId, Type = e.Type, Amount = e.Amount, Status = e.Status, CreatedAt = e.CreatedAt }).ToList();
        }

        public async Task ApproveExpenseAsync(string dispatchId, string expenseId, ApproveExpenseDto dto, string userId)
        {
            var exp = await _db.DispatchExpenses.FirstOrDefaultAsync(e => e.Id == expenseId && e.DispatchId == dispatchId);
            if (exp == null) throw new KeyNotFoundException("Expense not found");
            exp.Status = "approved";
            exp.ApprovedBy = dto.ApprovedBy;
            exp.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<MaterialDto> AddMaterialUsageAsync(string dispatchId, CreateMaterialUsageDto dto, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            var mat = new MaterialUsage
            {
                Id = Guid.NewGuid().ToString(),
                DispatchId = dispatchId,
                ArticleId = dto.ArticleId,
                Quantity = dto.Quantity,
                UsedBy = dto.UsedBy,
                UsedAt = DateTime.UtcNow,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            _db.DispatchMaterials.Add(mat);
            await _db.SaveChangesAsync();

            return new MaterialDto { Id = mat.Id, DispatchId = mat.DispatchId, ArticleId = mat.ArticleId, ArticleName = mat.ArticleName, Quantity = mat.Quantity, TotalPrice = mat.TotalPrice, Status = mat.Status, CreatedAt = mat.CreatedAt };
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsAsync(string dispatchId)
        {
            var items = await _db.DispatchMaterials.Where(m => m.DispatchId == dispatchId).ToListAsync();
            return items.Select(m => new MaterialDto { Id = m.Id, DispatchId = m.DispatchId, ArticleId = m.ArticleId, ArticleName = m.ArticleName, Quantity = m.Quantity, TotalPrice = m.TotalPrice, Status = m.Status, CreatedAt = m.CreatedAt }).ToList();
        }

        public async Task ApproveMaterialAsync(string dispatchId, string materialId, ApproveMaterialDto dto, string userId)
        {
            var m = await _db.DispatchMaterials.FirstOrDefaultAsync(x => x.Id == materialId && x.DispatchId == dispatchId);
            if (m == null) throw new KeyNotFoundException("Material not found");
            m.Status = "approved";
            m.ApprovedBy = dto.ApprovedBy;
            m.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<AttachmentUploadResponseDto> UploadAttachmentAsync(string dispatchId, Microsoft.AspNetCore.Http.IFormFile file, string category, string? description, double? latitude, double? longitude, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            // For scaffold: store metadata only. Storage integration (S3 etc.) is TODO.
            var att = new Attachment
            {
                Id = Guid.NewGuid().ToString(),
                DispatchId = dispatchId,
                FileName = file.FileName,
                FileType = file.ContentType,
                FileSizeMb = file.Length / 1024m / 1024m,
                Category = category,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow,
                StoragePath = null
            };
            _db.DispatchAttachments.Add(att);
            await _db.SaveChangesAsync();

            return new AttachmentUploadResponseDto { Id = att.Id, FileName = att.FileName, FileType = att.FileType, FileSizeMb = att.FileSizeMb, Category = att.Category, UploadedAt = att.UploadedAt };
        }

        public async Task<NoteDto> AddNoteAsync(string dispatchId, CreateNoteDto dto, string userId)
        {
            var d = await _db.Dispatches.FirstOrDefaultAsync(x => x.Id == dispatchId && !x.IsDeleted);
            if (d == null) throw new KeyNotFoundException($"Dispatch {dispatchId} not found");

            var note = new Note
            {
                Id = Guid.NewGuid().ToString(),
                DispatchId = dispatchId,
                Content = dto.Content,
                Category = dto.Category,
                Priority = dto.Priority,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.DispatchNotes.Add(note);
            await _db.SaveChangesAsync();
            return new NoteDto { Id = note.Id, Content = note.Content, Category = note.Category, Priority = note.Priority, CreatedBy = note.CreatedBy, CreatedAt = note.CreatedAt };
        }

        public async Task<DispatchStatisticsDto> GetStatisticsAsync(StatisticsQueryParams query)
        {
            var dispatches = _db.Dispatches.Where(d => !d.IsDeleted).AsQueryable();

            // Apply date filters
            if (query.DateFrom.HasValue)
                dispatches = dispatches.Where(d => d.CreatedAt >= query.DateFrom.Value);
            if (query.DateTo.HasValue)
                dispatches = dispatches.Where(d => d.CreatedAt <= query.DateTo.Value);

            if (!string.IsNullOrEmpty(query.Status))
                dispatches = dispatches.Where(d => d.Status == query.Status);

            if (!string.IsNullOrEmpty(query.TechnicianId))
                dispatches = dispatches.Where(d => d.AssignedTechnicians.Any(at => at.TechnicianId == query.TechnicianId));

            var dispatchList = await dispatches.ToListAsync();

            // Precompute duration values to avoid nullable value warnings and make intent explicit
            var durationValues = dispatchList.Where(d => d.ActualDuration.HasValue).Select(d => d.ActualDuration!.Value).ToList();
            var averageDurationValue = durationValues.Any()
                ? Math.Round(durationValues.Average(), 2)
                : 0;

            var stats = new DispatchStatisticsDto
            {
                TotalDispatches = dispatchList.Count,
                CompletedDispatches = dispatchList.Count(d => d.Status == "completed"),
                PendingDispatches = dispatchList.Count(d => d.Status == "pending"),
                InProgressDispatches = dispatchList.Count(d => d.Status == "in_progress"),
                CancelledDispatches = dispatchList.Count(d => d.Status == "cancelled"),
                CompletionRate = dispatchList.Count > 0 
                    ? Math.Round((decimal)dispatchList.Count(d => d.Status == "completed") / dispatchList.Count * 100, 2)
                    : 0,
                AverageDuration = averageDurationValue,
                TotalTechnicians = dispatchList.SelectMany(d => d.AssignedTechnicians).Select(t => t.TechnicianId).Distinct().Count(),
                HighPriorityCount = dispatchList.Count(d => d.Priority == "high"),
                MediumPriorityCount = dispatchList.Count(d => d.Priority == "medium"),
                LowPriorityCount = dispatchList.Count(d => d.Priority == "low"),
                GeneratedAt = DateTime.UtcNow
            };

            return stats;
        }
    }
}
