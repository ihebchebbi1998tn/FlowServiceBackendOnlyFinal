using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("service/api/InstallationRequest")]
    [Authorize]
    public class InstallationRequestController : ControllerBase
    {
        private readonly InstallationRequestService _installationRequestService;

        public InstallationRequestController(InstallationRequestService installationRequestService)
        {
            _installationRequestService = installationRequestService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var installationRequest = await _installationRequestService.GetByIdAsync(id);
                if (installationRequest == null)
                {
                    return NotFound(new { message = "Installation request not found" });
                }
                return Ok(installationRequest);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving installation request", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateInstallationRequestRequest installationRequest)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                var request = new InstallationRequest
                {
                    UserId = userId,
                    DeviceId = installationRequest.DeviceId,
                    RequestedDate = installationRequest.RequestedDate,
                    Status = "Pending",
                    Notes = installationRequest.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdRequest = await _installationRequestService.CreateAsync(request);
                return Ok(new { message = "Installation request created successfully", installationRequest = createdRequest });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating installation request", error = ex.Message });
            }
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle([FromForm] ToggleInstallationRequestRequest installationRequest)
        {
            try
            {
                var existingRequest = await _installationRequestService.GetByIdAsync(installationRequest.Id);
                if (existingRequest == null)
                {
                    return NotFound(new { message = "Installation request not found" });
                }

                // Toggle status
                existingRequest.Status = existingRequest.Status == "Active" ? "Inactive" : "Active";
                existingRequest.UpdatedAt = DateTime.UtcNow;

                await _installationRequestService.UpdateAsync(existingRequest);
                return Ok(new { message = "Installation request status toggled successfully", status = existingRequest.Status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error toggling installation request", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var installationRequest = await _installationRequestService.GetByIdAsync(id);
                if (installationRequest == null)
                {
                    return NotFound(new { message = "Installation request not found" });
                }

                await _installationRequestService.DeleteAsync(id);
                return Ok(new { message = "Installation request deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting installation request", error = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadInvoice(string id, [FromQuery] string lang, [FromQuery] string city, [FromQuery] string streetName, [FromQuery] string zipCode, [FromQuery] string phone, [FromQuery] string email)
        {
            try
            {
                // TODO: Implement PDF generation logic using QuestPDF
                return Ok(new { message = "PDF generation not yet implemented", installationRequestId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating PDF", error = ex.Message });
            }
        }
    }

    public class CreateInstallationRequestRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ToggleInstallationRequestRequest
    {
        public string Id { get; set; } = string.Empty;
    }
}