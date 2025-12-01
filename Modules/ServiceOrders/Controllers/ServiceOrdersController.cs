using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Modules.ServiceOrders.DTOs;
using MyApi.Modules.ServiceOrders.Services;
using System.Security.Claims;

namespace MyApi.Modules.ServiceOrders.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/service-orders")]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IServiceOrderService _serviceOrderService;
        private readonly ILogger<ServiceOrdersController> _logger;

        public ServiceOrdersController(IServiceOrderService serviceOrderService, ILogger<ServiceOrdersController> logger)
        {
            _serviceOrderService = serviceOrderService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        }

        [HttpPost("from-sale/{saleId}")]
        public async Task<IActionResult> CreateFromSale(string saleId, [FromBody] CreateServiceOrderDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var serviceOrder = await _serviceOrderService.CreateFromSaleAsync(saleId, createDto, userId);
                return CreatedAtAction(nameof(GetServiceOrderById), new { id = serviceOrder.Id }, new
                {
                    success = true,
                    data = serviceOrder
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = ex.Message } });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = new { code = "CONFLICT", message = ex.Message } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service order from sale {SaleId}", saleId);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceOrders(
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] int? contact_id = null,
            [FromQuery] string? sale_id = null,
            [FromQuery] DateTime? start_date = null,
            [FromQuery] DateTime? end_date = null,
            [FromQuery] string? payment_status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "created_at",
            [FromQuery] string sortOrder = "desc"
        )
        {
            try
            {
                var result = await _serviceOrderService.GetServiceOrdersAsync(
                    status, priority, contact_id, sale_id, start_date, end_date, payment_status, search,
                    page, pageSize, sortBy, sortOrder
                );
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service orders");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceOrderById(string id, [FromQuery] bool includeJobs = true)
        {
            try
            {
                var serviceOrder = await _serviceOrderService.GetServiceOrderByIdAsync(id, includeJobs);
                if (serviceOrder == null)
                    return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });

                return Ok(new { success = true, data = serviceOrder });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceOrder(string id, [FromBody] UpdateServiceOrderDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var serviceOrder = await _serviceOrderService.UpdateServiceOrderAsync(id, updateDto, userId);
                return Ok(new { success = true, data = serviceOrder });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchServiceOrder(string id, [FromBody] UpdateServiceOrderDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var serviceOrder = await _serviceOrderService.PatchServiceOrderAsync(id, updateDto, userId);
                return Ok(new { success = true, data = serviceOrder });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateServiceOrderStatusDto statusDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var serviceOrder = await _serviceOrderService.UpdateStatusAsync(id, statusDto, userId);
                return Ok(new { success = true, data = serviceOrder });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = new { code = "INVALID_STATUS_TRANSITION", message = ex.Message } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id, [FromBody] ApproveServiceOrderDto approveDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var serviceOrder = await _serviceOrderService.ApproveAsync(id, approveDto, userId);
                return Ok(new { success = true, data = serviceOrder });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = new { code = "INVALID_REQUEST", message = ex.Message } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(string id, [FromBody] CompleteServiceOrderDto completeDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var serviceOrder = await _serviceOrderService.CompleteAsync(id, completeDto, userId);
                return Ok(new { success = true, data = serviceOrder });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = new { code = "INVALID_REQUEST", message = ex.Message } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(string id, [FromBody] CancelServiceOrderDto cancelDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var serviceOrder = await _serviceOrderService.CancelAsync(id, cancelDto, userId);
                return Ok(new { success = true, data = serviceOrder });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _serviceOrderService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Service order not found" } });

                return Ok(new { success = true, message = "Service order deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = new { code = "INVALID_REQUEST", message = ex.Message } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service order {Id}", id);
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] int? contactId = null
        )
        {
            try
            {
                var stats = await _serviceOrderService.GetStatisticsAsync(startDate, endDate, status, contactId);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service order statistics");
                return StatusCode(500, new { success = false, error = new { code = "INTERNAL_ERROR", message = "An error occurred" } });
            }
        }
    }
}
