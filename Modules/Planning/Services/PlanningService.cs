using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApi.Data;
using MyApi.Modules.Planning.DTOs;
using MyApi.Modules.Planning.Models;
using MyApi.Modules.ServiceOrders.Models;
using MyApi.Modules.Dispatches.Models;
using MyApi.Modules.Dispatches.Services;
using MyApi.Modules.Dispatches.DTOs;

namespace MyApi.Modules.Planning.Services
{
    public class PlanningService : IPlanningService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDispatchService _dispatchService;
        private readonly ILogger<PlanningService> _logger;

        public PlanningService(
            ApplicationDbContext db,
            IDispatchService dispatchService,
            ILogger<PlanningService> logger)
        {
            _db = db;
            _dispatchService = dispatchService;
            _logger = logger;
        }

        public async Task<AssignJobResponseDto> AssignJobAsync(AssignJobDto dto, string userId)
        {
            // 1. Validate assignment
            var validation = await ValidateAssignmentAsync(new ValidateAssignmentDto
            {
                JobId = dto.JobId,
                TechnicianIds = dto.TechnicianIds,
                ScheduledDate = dto.ScheduledDate,
                ScheduledStartTime = dto.ScheduledStartTime,
                ScheduledEndTime = dto.ScheduledEndTime
            });

            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Assignment validation failed: {string.Join(", ", validation.Conflicts.Select(c => c.Message))}");
            }

            // 2. Update job
            var job = await _db.ServiceOrderJobs.FirstOrDefaultAsync(j => j.Id == dto.JobId);
            if (job == null)
                throw new KeyNotFoundException($"Job {dto.JobId} not found");

            job.AssignedTechnicianIds = dto.TechnicianIds.ToArray();
            job.ScheduledDate = dto.ScheduledDate;
            job.ScheduledStartTime = dto.ScheduledStartTime;
            job.ScheduledEndTime = dto.ScheduledEndTime;
            job.Status = "scheduled";
            job.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // 3. Create dispatch if requested
            object? dispatch = null;
            if (dto.AutoCreateDispatch)
            {
                try
                {
                    var createDispatchDto = new CreateDispatchFromJobDto
                    {
                        AssignedTechnicianIds = dto.TechnicianIds,
                        ScheduledDate = dto.ScheduledDate,
                        ScheduledStartTime = dto.ScheduledStartTime,
                        ScheduledEndTime = dto.ScheduledEndTime,
                        Priority = dto.Priority
                    };

                    dispatch = await _dispatchService.CreateFromJobAsync(dto.JobId, createDispatchDto, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create dispatch for job {JobId}", dto.JobId);
                    // Continue without dispatch creation
                }
            }

            // 4. Return response
            return new AssignJobResponseDto
            {
                Job = MapJobToDto(job),
                Dispatch = dispatch
            };
        }

        public async Task<BatchAssignResponseDto> BatchAssignAsync(BatchAssignDto dto, string userId)
        {
            var response = new BatchAssignResponseDto();

            foreach (var assignment in dto.Assignments)
            {
                try
                {
                    assignment.AutoCreateDispatch = dto.AutoCreateDispatches;
                    var result = await AssignJobAsync(assignment, userId);
                    
                    response.Successful++;
                    response.Results.Add(new BatchAssignResult
                    {
                        JobId = assignment.JobId,
                        Status = "success",
                        DispatchId = result.Dispatch != null ? (result.Dispatch as DispatchDto)?.Id : null
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to assign job {JobId} in batch", assignment.JobId);
                    response.Failed++;
                    response.Results.Add(new BatchAssignResult
                    {
                        JobId = assignment.JobId,
                        Status = "failed",
                        ErrorMessage = ex.Message
                    });
                }
            }

            return response;
        }

        public async Task<AssignmentValidationResult> ValidateAssignmentAsync(ValidateAssignmentDto dto)
        {
            var result = new AssignmentValidationResult { IsValid = true };

            // 1. Check job exists
            var job = await _db.ServiceOrderJobs.FirstOrDefaultAsync(j => j.Id == dto.JobId);
            if (job == null)
            {
                result.IsValid = false;
                result.Conflicts.Add(new AssignmentConflict
                {
                    Type = "job_not_found",
                    Message = $"Job {dto.JobId} not found"
                });
                return result;
            }

            // 2. Check each technician
            foreach (var techId in dto.TechnicianIds)
            {
                if (!int.TryParse(techId, out int technicianId))
                {
                    result.IsValid = false;
                    result.Conflicts.Add(new AssignmentConflict
                    {
                        Type = "invalid_technician_id",
                        Message = $"Invalid technician ID: {techId}"
                    });
                    continue;
                }

                var technician = await _db.Users.FirstOrDefaultAsync(u => u.Id == technicianId && u.Role == "technician");
                if (technician == null)
                {
                    result.IsValid = false;
                    result.Conflicts.Add(new AssignmentConflict
                    {
                        Type = "technician_not_found",
                        Message = $"Technician {techId} not found"
                    });
                    continue;
                }

                // 3. Check technician on leave
                var onLeave = await _db.Set<TechnicianLeave>()
                    .AnyAsync(l =>
                        l.TechnicianId == technicianId &&
                        l.Status == "approved" &&
                        l.StartDate <= dto.ScheduledDate.Date &&
                        l.EndDate >= dto.ScheduledDate.Date);

                if (onLeave)
                {
                    result.IsValid = false;
                    result.Conflicts.Add(new AssignmentConflict
                    {
                        Type = "on_leave",
                        Message = $"Technician {technician.FirstName} {technician.LastName} is on leave on {dto.ScheduledDate:yyyy-MM-dd}"
                    });
                    continue;
                }

                // 4. Check time conflicts with existing dispatches
                var conflictingDispatches = await _db.Dispatches
                    .Include(d => d.AssignedTechnicians)
                    .Where(d =>
                        d.AssignedTechnicians.Any(at => at.TechnicianId == techId) &&
                        d.ScheduledDate == dto.ScheduledDate.Date &&
                        !d.IsDeleted &&
                        d.Status != "cancelled" &&
                        d.Status != "completed")
                    .ToListAsync();

                foreach (var cd in conflictingDispatches)
                {
                    if (cd.ScheduledStartTime.HasValue && cd.ScheduledEndTime.HasValue)
                    {
                        bool overlaps =
                            (dto.ScheduledStartTime >= cd.ScheduledStartTime.Value && dto.ScheduledStartTime < cd.ScheduledEndTime.Value) ||
                            (dto.ScheduledEndTime > cd.ScheduledStartTime.Value && dto.ScheduledEndTime <= cd.ScheduledEndTime.Value) ||
                            (dto.ScheduledStartTime <= cd.ScheduledStartTime.Value && dto.ScheduledEndTime >= cd.ScheduledEndTime.Value);

                        if (overlaps)
                        {
                            result.IsValid = false;
                            result.Conflicts.Add(new AssignmentConflict
                            {
                                Type = "time_conflict",
                                Message = $"Technician {technician.FirstName} {technician.LastName} already has dispatch {cd.DispatchNumber} from {cd.ScheduledStartTime:hh\\:mm}-{cd.ScheduledEndTime:hh\\:mm}",
                                ConflictingData = new
                                {
                                    cd.Id,
                                    cd.DispatchNumber,
                                    ScheduledStartTime = cd.ScheduledStartTime.ToString(),
                                    ScheduledEndTime = cd.ScheduledEndTime.ToString()
                                }
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<PagedResult<ServiceOrderJobDto>> GetUnassignedJobsAsync(
            string? priority,
            List<string>? requiredSkills,
            string? serviceOrderId,
            int page,
            int pageSize)
        {
            var query = _db.ServiceOrderJobs
                .Where(j => j.Status == "unscheduled" || j.Status == "unassigned");

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(j => j.Priority == priority);

            if (!string.IsNullOrEmpty(serviceOrderId))
                query = query.Where(j => j.ServiceOrderId == serviceOrderId);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ServiceOrderJobDto>
            {
                Data = items.Select(MapJobToDto).ToList(),
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<TechnicianScheduleDto> GetTechnicianScheduleAsync(
            string technicianId,
            DateTime startDate,
            DateTime endDate)
        {
            if (!int.TryParse(technicianId, out int techId))
                throw new ArgumentException("Invalid technician ID");

            var technician = await _db.Users.FirstOrDefaultAsync(u => u.Id == techId);
            if (technician == null)
                throw new KeyNotFoundException($"Technician {technicianId} not found");

            // Get working hours
            var workingHours = await _db.Set<TechnicianWorkingHours>()
                .Where(wh => wh.TechnicianId == techId && wh.IsActive)
                .ToListAsync();

            // Get dispatches
            var dispatches = await _db.Dispatches
                .Include(d => d.AssignedTechnicians)
                .Where(d =>
                    d.AssignedTechnicians.Any(at => at.TechnicianId == technicianId) &&
                    d.ScheduledDate >= startDate.Date &&
                    d.ScheduledDate <= endDate.Date &&
                    !d.IsDeleted)
                .ToListAsync();

            // Get leaves
            var leaves = await _db.Set<TechnicianLeave>()
                .Where(l =>
                    l.TechnicianId == techId &&
                    l.Status == "approved" &&
                    l.StartDate <= endDate.Date &&
                    l.EndDate >= startDate.Date)
                .ToListAsync();

            // Build response
            return new TechnicianScheduleDto
            {
                TechnicianId = technicianId,
                TechnicianName = $"{technician.FirstName} {technician.LastName}",
                WorkingHours = BuildWorkingHoursDict(workingHours),
                Dispatches = dispatches.Select(MapDispatchToScheduleDto).ToList(),
                Leaves = leaves.Select(MapLeaveToDto).ToList(),
                TotalScheduledHours = CalculateTotalScheduledHours(dispatches),
                AvailableHours = CalculateAvailableHours(workingHours, dispatches, startDate, endDate)
            };
        }

        public async Task<List<TechnicianAvailabilityDto>> GetAvailableTechniciansAsync(
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            List<string>? requiredSkills)
        {
            var technicians = await _db.Users
                .Where(u => u.Role == "technician" && u.IsActive)
                .ToListAsync();

            var availabilityList = new List<TechnicianAvailabilityDto>();

            foreach (var tech in technicians)
            {
                // Check if on leave
                var onLeave = await _db.Set<TechnicianLeave>()
                    .AnyAsync(l =>
                        l.TechnicianId == tech.Id &&
                        l.Status == "approved" &&
                        l.StartDate <= date.Date &&
                        l.EndDate >= date.Date);

                if (onLeave) continue;

                // Get scheduled dispatches for that day
                var dispatches = await _db.Dispatches
                    .Include(d => d.AssignedTechnicians)
                    .Where(d =>
                        d.AssignedTechnicians.Any(at => at.TechnicianId == tech.Id.ToString()) &&
                        d.ScheduledDate == date.Date &&
                        !d.IsDeleted &&
                        d.Status != "cancelled" &&
                        d.Status != "completed")
                    .ToListAsync();

                // Calculate availability
                var scheduledMinutes = dispatches
                    .Where(d => d.ScheduledStartTime.HasValue && d.ScheduledEndTime.HasValue)
                    .Sum(d => (d.ScheduledEndTime!.Value - d.ScheduledStartTime!.Value).TotalMinutes);

                // Get working hours for this day
                var dayOfWeek = (int)date.DayOfWeek;
                var workingHours = await _db.Set<TechnicianWorkingHours>()
                    .FirstOrDefaultAsync(wh => wh.TechnicianId == tech.Id && wh.DayOfWeek == dayOfWeek && wh.IsActive);

                var availableMinutes = workingHours != null
                    ? (workingHours.EndTime - workingHours.StartTime).TotalMinutes - scheduledMinutes
                    : 0;

                var isAvailable = availableMinutes >= (endTime - startTime).TotalMinutes;

                availabilityList.Add(new TechnicianAvailabilityDto
                {
                    Id = tech.Id.ToString(),
                    Name = $"{tech.FirstName} {tech.LastName}",
                    Email = tech.Email,
                    Skills = tech.Skills?.ToList() ?? new List<string>(),
                    Status = tech.CurrentStatus ?? "offline",
                    IsAvailable = isAvailable,
                    AvailableMinutes = (int)availableMinutes,
                    ScheduledMinutes = (int)scheduledMinutes,
                    UtilizationPercentage = workingHours != null
                        ? (decimal)(scheduledMinutes / (workingHours.EndTime - workingHours.StartTime).TotalMinutes * 100)
                        : 0
                });
            }

            return availabilityList.OrderByDescending(a => a.IsAvailable).ThenBy(a => a.ScheduledMinutes).ToList();
        }

        // Helper methods
        private ServiceOrderJobDto MapJobToDto(ServiceOrderJob job)
        {
            LocationDto? location = null;
            if (!string.IsNullOrEmpty(job.LocationJson))
            {
                try
                {
                    var locData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(job.LocationJson);
                    if (locData != null)
                    {
                        location = new LocationDto
                        {
                            Address = locData.ContainsKey("address") ? locData["address"]?.ToString() ?? "" : "",
                            Lat = locData.ContainsKey("lat") && double.TryParse(locData["lat"]?.ToString(), out var lat) ? lat : null,
                            Lng = locData.ContainsKey("lng") && double.TryParse(locData["lng"]?.ToString(), out var lng) ? lng : null
                        };
                    }
                }
                catch { }
            }

            return new ServiceOrderJobDto
            {
                Id = job.Id,
                ServiceOrderId = job.ServiceOrderId,
                Title = job.Title,
                Description = job.Description,
                Status = job.Status,
                Priority = job.Priority ?? "medium",
                EstimatedDuration = job.EstimatedDuration,
                RequiredSkills = job.RequiredSkills?.ToList(),
                AssignedTechnicianIds = job.AssignedTechnicianIds?.ToList() ?? new List<string>(),
                ScheduledDate = job.ScheduledDate,
                ScheduledStartTime = job.ScheduledStartTime,
                ScheduledEndTime = job.ScheduledEndTime,
                Location = location,
                CustomerName = job.CustomerName,
                CustomerPhone = job.CustomerPhone,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt
            };
        }

        private Dictionary<string, WorkingHoursDto?> BuildWorkingHoursDict(List<TechnicianWorkingHours> hours)
        {
            var days = new[] { "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };
            var dict = new Dictionary<string, WorkingHoursDto?>();

            foreach (var day in days.Select((name, index) => (name, index)))
            {
                var wh = hours.FirstOrDefault(h => h.DayOfWeek == day.index);
                dict[day.name] = wh == null ? null : new WorkingHoursDto
                {
                    Start = wh.StartTime.ToString(@"hh\:mm"),
                    End = wh.EndTime.ToString(@"hh\:mm")
                };
            }

            return dict;
        }

        private DispatchScheduleDto MapDispatchToScheduleDto(Dispatch dispatch)
        {
            return new DispatchScheduleDto
            {
                Id = dispatch.Id,
                DispatchNumber = dispatch.DispatchNumber,
                JobId = dispatch.JobId ?? "",
                JobTitle = dispatch.DispatchNumber, // Use dispatch number as title
                ServiceOrderId = dispatch.ServiceOrderId ?? "",
                ScheduledDate = dispatch.ScheduledDate ?? DateTime.MinValue,
                ScheduledStartTime = dispatch.ScheduledStartTime ?? TimeSpan.Zero,
                ScheduledEndTime = dispatch.ScheduledEndTime ?? TimeSpan.Zero,
                EstimatedDuration = dispatch.EstimatedDuration ?? 0,
                Status = dispatch.Status,
                Priority = dispatch.Priority
            };
        }

        private TechnicianLeaveDto MapLeaveToDto(TechnicianLeave leave)
        {
            return new TechnicianLeaveDto
            {
                Id = leave.Id,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Status = leave.Status
            };
        }

        private decimal CalculateTotalScheduledHours(List<Dispatch> dispatches)
        {
            decimal total = 0;
            foreach (var d in dispatches)
            {
                if (d.ScheduledStartTime.HasValue && d.ScheduledEndTime.HasValue)
                {
                    total += (decimal)(d.ScheduledEndTime.Value - d.ScheduledStartTime.Value).TotalHours;
                }
            }
            return total;
        }

        private decimal CalculateAvailableHours(List<TechnicianWorkingHours> workingHours, List<Dispatch> dispatches, DateTime startDate, DateTime endDate)
        {
            // Simplified calculation - count working days and subtract scheduled hours
            var totalWorkingHours = 0m;
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayOfWeek = (int)currentDate.DayOfWeek;
                var wh = workingHours.FirstOrDefault(w => w.DayOfWeek == dayOfWeek);
                if (wh != null)
                {
                    totalWorkingHours += (decimal)(wh.EndTime - wh.StartTime).TotalHours;
                }
                currentDate = currentDate.AddDays(1);
            }

            return totalWorkingHours - CalculateTotalScheduledHours(dispatches);
        }
    }
}
