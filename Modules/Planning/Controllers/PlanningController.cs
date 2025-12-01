using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyApi.Modules.Planning.DTOs;
using MyApi.Modules.Planning.Services;

namespace MyApi.Modules.Planning.Controllers
{
    [ApiController]
    [Route("api/planning")]
    public class PlanningController : ControllerBase
    {
        private readonly IPlanningService _planningService;
        private readonly ILogger<PlanningController> _logger;

        public PlanningController(IPlanningService planningService, ILogger<PlanningController> logger)
        {
            _planningService = planningService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        }

        /// <summary>
        /// Get unassigned jobs ready for scheduling
        /// </summary>
        [HttpGet("unassigned-jobs")]
        public async Task<IActionResult> GetUnassignedJobs(
            [FromQuery] string? priority = null,
            [FromQuery] string? required_skills = null,
            [FromQuery] string? service_order_id = null,
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 20)
        {
            try
            {
                var skills = string.IsNullOrEmpty(required_skills)
                    ? null
                    : required_skills.Split(',').ToList();

                var result = await _planningService.GetUnassignedJobsAsync(
                    priority, skills, service_order_id, page, page_size);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unassigned jobs");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = ex.Message } });
            }
        }

        /// <summary>
        /// Assign a job to one or more technicians with schedule
        /// </summary>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignJob([FromBody] AssignJobDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _planningService.AssignJobAsync(dto, userId);
                return Ok(new { success = true, data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = ex.Message } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning job");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = ex.Message } });
            }
        }

        /// <summary>
        /// Assign multiple jobs at once
        /// </summary>
        [HttpPost("batch-assign")]
        public async Task<IActionResult> BatchAssign([FromBody] BatchAssignDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _planningService.BatchAssignAsync(dto, userId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch assigning jobs");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = ex.Message } });
            }
        }

        /// <summary>
        /// Validate assignment before committing (checks conflicts, availability, skills)
        /// </summary>
        [HttpPost("validate-assignment")]
        public async Task<IActionResult> ValidateAssignment([FromBody] ValidateAssignmentDto dto)
        {
            try
            {
                var result = await _planningService.ValidateAssignmentAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating assignment");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = ex.Message } });
            }
        }

        /// <summary>
        /// Get technician's schedule for a date range
        /// </summary>
        [HttpGet("technician-schedule/{technicianId}")]
        public async Task<IActionResult> GetTechnicianSchedule(
            string technicianId,
            [FromQuery] string start_date,
            [FromQuery] string end_date)
        {
            try
            {
                if (!DateTime.TryParse(start_date, out var startDate) ||
                    !DateTime.TryParse(end_date, out var endDate))
                {
                    return BadRequest(new { success = false, error = new { code = "INVALID_DATES", message = "Invalid date format" } });
                }

                var result = await _planningService.GetTechnicianScheduleAsync(technicianId, startDate, endDate);
                return Ok(new { success = true, data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = ex.Message } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching technician schedule");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = ex.Message } });
            }
        }

        /// <summary>
        /// Get available technicians for a specific time slot
        /// </summary>
        [HttpGet("available-technicians")]
        public async Task<IActionResult> GetAvailableTechnicians(
            [FromQuery] string date,
            [FromQuery] string start_time,
            [FromQuery] string end_time,
            [FromQuery] string? required_skills = null)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate) ||
                    !TimeSpan.TryParse(start_time, out var startTimeSpan) ||
                    !TimeSpan.TryParse(end_time, out var endTimeSpan))
                {
                    return BadRequest(new { success = false, error = new { code = "INVALID_PARAMETERS", message = "Invalid date or time format" } });
                }

                var skills = string.IsNullOrEmpty(required_skills)
                    ? null
                    : required_skills.Split(',').ToList();

                var result = await _planningService.GetAvailableTechniciansAsync(
                    parsedDate, startTimeSpan, endTimeSpan, skills);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available technicians");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = ex.Message } });
            }
        }
    }
}
