using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyApi.Modules.Dispatches.DTOs;
using MyApi.Modules.Dispatches.Services;

namespace MyApi.Modules.Dispatches.Controllers
{
    [ApiController]
    [Route("api/dispatches")]
    [Authorize]
    public class DispatchesController : ControllerBase
    {
        private readonly IDispatchService _service;
        private readonly ILogger<DispatchesController> _logger;

        public DispatchesController(IDispatchService service, ILogger<DispatchesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("from-job/{jobId}")]
        public async Task<IActionResult> CreateFromJob(string jobId, [FromBody] CreateDispatchFromJobDto dto)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.CreateFromJobAsync(jobId, dto, userId);
            return CreatedAtAction(nameof(GetById), new { dispatchId = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DispatchQueryParams query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{dispatchId}")]
        public async Task<IActionResult> GetById(string dispatchId)
        {
            var result = await _service.GetByIdAsync(dispatchId);
            return Ok(result);
        }

        [HttpPut("{dispatchId}")]
        public async Task<IActionResult> Update(string dispatchId, [FromBody] UpdateDispatchDto dto)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.UpdateAsync(dispatchId, dto, userId);
            return Ok(result);
        }

        [HttpPatch("{dispatchId}/status")]
        public async Task<IActionResult> UpdateStatus(string dispatchId, [FromBody] UpdateDispatchStatusDto dto)
        {
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.UpdateStatusAsync(dispatchId, dto, userId);
            return Ok(result);
        }

        [HttpPost("{dispatchId}/start")]
        public async Task<IActionResult> Start(string dispatchId, [FromBody] StartDispatchDto dto)
        {
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.StartDispatchAsync(dispatchId, dto, userId);
            return Ok(result);
        }

        [HttpPost("{dispatchId}/complete")]
        public async Task<IActionResult> Complete(string dispatchId, [FromBody] CompleteDispatchDto dto)
        {
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.CompleteDispatchAsync(dispatchId, dto, userId);
            return Ok(result);
        }

        [HttpPost("{dispatchId}/cancel")]
        public async Task<IActionResult> Cancel(string dispatchId, [FromBody] CancelDispatchDto dto)
        {
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.UpdateStatusAsync(dispatchId, new UpdateDispatchStatusDto { Status = "cancelled", Notes = dto.Notes }, userId);
            return Ok(result);
        }

        [HttpDelete("{dispatchId}")]
        public async Task<IActionResult> Delete(string dispatchId)
        {
            var userId = User?.Identity?.Name ?? "system";
            await _service.DeleteAsync(dispatchId, userId);
            return NoContent();
        }

        // Time entries
        [HttpPost("{dispatchId}/time-entries")]
        public async Task<IActionResult> AddTimeEntry(string dispatchId, [FromBody] CreateTimeEntryDto dto)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.AddTimeEntryAsync(dispatchId, dto, userId);
            return CreatedAtAction(nameof(GetTimeEntries), new { dispatchId }, result);
        }

        [HttpGet("{dispatchId}/time-entries")]
        public async Task<IActionResult> GetTimeEntries(string dispatchId)
        {
            var result = await _service.GetTimeEntriesAsync(dispatchId);
            return Ok(new { data = result });
        }

        [HttpPost("{dispatchId}/time-entries/{timeEntryId}/approve")]
        public async Task<IActionResult> ApproveTimeEntry(string dispatchId, string timeEntryId, [FromBody] ApproveTimeEntryDto dto)
        {
            var userId = User?.Identity?.Name ?? "system";
            await _service.ApproveTimeEntryAsync(dispatchId, timeEntryId, dto, userId);
            return Ok(new { id = timeEntryId, status = "approved" });
        }

        // Expenses
        [HttpPost("{dispatchId}/expenses")]
        public async Task<IActionResult> AddExpense(string dispatchId, [FromBody] CreateExpenseDto dto)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.AddExpenseAsync(dispatchId, dto, userId);
            return CreatedAtAction(nameof(GetExpenses), new { dispatchId }, result);
        }

        [HttpGet("{dispatchId}/expenses")]
        public async Task<IActionResult> GetExpenses(string dispatchId)
        {
            var result = await _service.GetExpensesAsync(dispatchId);
            return Ok(new { data = result });
        }

        [HttpPost("{dispatchId}/expenses/{expenseId}/approve")]
        public async Task<IActionResult> ApproveExpense(string dispatchId, string expenseId, [FromBody] ApproveExpenseDto dto)
        {
            var userId = User?.Identity?.Name ?? "system";
            await _service.ApproveExpenseAsync(dispatchId, expenseId, dto, userId);
            return Ok(new { id = expenseId, status = "approved" });
        }

        // Materials
        [HttpPost("{dispatchId}/materials")]
        public async Task<IActionResult> AddMaterial(string dispatchId, [FromBody] CreateMaterialUsageDto dto)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.AddMaterialUsageAsync(dispatchId, dto, userId);
            return CreatedAtAction(nameof(GetMaterials), new { dispatchId }, result);
        }

        [HttpGet("{dispatchId}/materials")]
        public async Task<IActionResult> GetMaterials(string dispatchId)
        {
            var result = await _service.GetMaterialsAsync(dispatchId);
            return Ok(new { data = result });
        }

        [HttpPost("{dispatchId}/materials/{materialId}/approve")]
        public async Task<IActionResult> ApproveMaterial(string dispatchId, string materialId, [FromBody] ApproveMaterialDto dto)
        {
            var userId = User?.Identity?.Name ?? "system";
            await _service.ApproveMaterialAsync(dispatchId, materialId, dto, userId);
            return Ok(new { id = materialId, status = "approved" });
        }

    // Attachments
        [HttpPost("{dispatchId}/attachments")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(string dispatchId, [FromForm] MyApi.Modules.Dispatches.DTOs.AttachmentUploadDto dto)
        {
            var file = dto?.File;
            if (file == null) return BadRequest(new { error = "file is required" });
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.UploadAttachmentAsync(dispatchId, file, dto?.Category ?? "", dto?.Description, dto?.Latitude, dto?.Longitude, userId);
            return CreatedAtAction(nameof(GetById), new { dispatchId }, result);
        }

        // Notes
        [HttpPost("{dispatchId}/notes")]
        public async Task<IActionResult> AddNote(string dispatchId, [FromBody] CreateNoteDto dto)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
            var userId = User?.Identity?.Name ?? "system";
            var result = await _service.AddNoteAsync(dispatchId, dto, userId);
            return CreatedAtAction(nameof(GetById), new { dispatchId }, result);
        }

        // Statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] MyApi.Modules.Dispatches.DTOs.StatisticsQueryParams query)
        {
            var result = await _service.GetStatisticsAsync(query);
            return Ok(result);
        }
    }
}
