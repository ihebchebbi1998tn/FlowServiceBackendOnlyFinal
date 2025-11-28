using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("service/api/ServiceRequest")]
    [Authorize]
    public class ServiceRequestController : ControllerBase
    {
        private readonly ServiceRequestService _serviceRequestService;

        public ServiceRequestController(ServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateServiceRequestRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var serviceRequest = new ServiceRequest
                {
                    Title = dto.Title,
                    Description = dto.Description ?? string.Empty,
                    Status = "Pending",
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdServiceRequest = await _serviceRequestService.CreateAsync(serviceRequest);
                return Ok(new { message = "Service request created successfully", serviceRequest = createdServiceRequest });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating service request", error = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateServiceRequestRequest dto)
        {
            try
            {
                var existingServiceRequest = await _serviceRequestService.GetByIdAsync(dto.Id);
                if (existingServiceRequest == null)
                {
                    return NotFound(new { message = "Service request not found" });
                }

                existingServiceRequest.Title = dto.Title;
                existingServiceRequest.Description = dto.Description ?? existingServiceRequest.Description;
                existingServiceRequest.Status = dto.Status;
                existingServiceRequest.UpdatedAt = DateTime.UtcNow;

                var updatedServiceRequest = await _serviceRequestService.UpdateAsync(existingServiceRequest);
                return Ok(new { message = "Service request updated successfully", serviceRequest = updatedServiceRequest });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating service request", error = ex.Message });
            }
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle([FromForm] ToggleServiceRequestRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var serviceRequest = await _serviceRequestService.GetByIdAsync(dto.Id);
                if (serviceRequest == null)
                {
                    return NotFound(new { message = "Service request not found" });
                }

                // Toggle status (e.g., Active/Inactive or Pending/Completed)
                serviceRequest.Status = serviceRequest.Status == "Active" ? "Inactive" : "Active";
                serviceRequest.UpdatedAt = DateTime.UtcNow;

                await _serviceRequestService.UpdateAsync(serviceRequest);
                return Ok(new { message = "Service request status toggled successfully", status = serviceRequest.Status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error toggling service request status", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var serviceRequest = await _serviceRequestService.GetByIdAsync(id);
                if (serviceRequest == null)
                {
                    return NotFound(new { message = "Service request not found" });
                }

                await _serviceRequestService.DeleteAsync(id);
                return Ok(new { message = "Service request deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting service request", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var data = await _serviceRequestService.GetByUserIdAsync(userId, searchTerm, page, pageSize);
                var totalCount = await _serviceRequestService.GetTotalCountByUserIdAsync(userId, searchTerm);

                return Ok(new { totalCount, page, pageSize, data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving service requests", error = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadInvoice(int id, [FromQuery] string lang, [FromQuery] string city, [FromQuery] string streetName, [FromQuery] string zipCode, [FromQuery] string phone, [FromQuery] string email)
        {
            try
            {
                // TODO: Implement PDF generation logic using QuestPDF
                return Ok(new { message = "PDF generation not yet implemented", serviceRequestId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating PDF", error = ex.Message });
            }
        }
    }

    public class CreateServiceRequestRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateServiceRequestRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ToggleServiceRequestRequest
    {
        public string Id { get; set; } = string.Empty;
    }
}