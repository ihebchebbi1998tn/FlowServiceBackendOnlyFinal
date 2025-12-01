using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyApi.Modules.Planning.DTOs;
using MyApi.Modules.Dispatches.DTOs;

namespace MyApi.Modules.Planning.Services
{
    public interface IPlanningService
    {
        // Job Assignment
        Task<AssignJobResponseDto> AssignJobAsync(AssignJobDto dto, string userId);
        Task<BatchAssignResponseDto> BatchAssignAsync(BatchAssignDto dto, string userId);
        Task<AssignmentValidationResult> ValidateAssignmentAsync(ValidateAssignmentDto dto);

        // Unassigned Jobs
        Task<PagedResult<ServiceOrderJobDto>> GetUnassignedJobsAsync(
            string? priority,
            List<string>? requiredSkills,
            string? serviceOrderId,
            int page,
            int pageSize
        );

        // Technician Schedule
        Task<TechnicianScheduleDto> GetTechnicianScheduleAsync(
            string technicianId,
            DateTime startDate,
            DateTime endDate
        );

        // Available Technicians
        Task<List<TechnicianAvailabilityDto>> GetAvailableTechniciansAsync(
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            List<string>? requiredSkills
        );
    }
}
