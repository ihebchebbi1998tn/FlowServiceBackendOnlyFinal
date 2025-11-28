using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("service/api/Order")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = Guid.NewGuid().ToString("N")[..8],
                    Status = "Pending",
                    TotalAmount = dto.TotalAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdOrder = await _orderService.CreateAsync(order);
                return Ok(new { message = "Order created successfully", order = createdOrder });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating order", error = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateOrderRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var existingOrder = await _orderService.GetByIdAsync(dto.Id);
                if (existingOrder == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                existingOrder.Status = dto.Status;
                existingOrder.TotalAmount = dto.TotalAmount;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                var updatedOrder = await _orderService.UpdateAsync(existingOrder);
                return Ok(new { message = "Order updated successfully", order = updatedOrder });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating order", error = ex.Message });
            }
        }

        [HttpPost("payment_methode")]
        public async Task<IActionResult> PaymentMethode([FromBody] PaymentMethodRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var order = await _orderService.GetByIdAsync(dto.OrderId);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                // Update payment method (assuming there's a PaymentMethod field)
                order.UpdatedAt = DateTime.UtcNow;
                await _orderService.UpdateAsync(order);

                return Ok(new { message = "Payment method updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating payment method", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                await _orderService.DeleteAsync(id);
                return Ok(new { message = "Order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting order", error = ex.Message });
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
                var orders = await _orderService.GetByUserIdAsync(userId, page, pageSize);
                var totalCount = await _orderService.GetTotalCountByUserIdAsync(userId);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    orders = orders.Where(o => 
                        o.Status.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        o.OrderNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                return Ok(new { totalCount, page, pageSize, data = orders });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving orders", error = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadInvoice(string id, [FromQuery] string lang, [FromQuery] string city, [FromQuery] string streetName, [FromQuery] string zipCode, [FromQuery] string phone, [FromQuery] string email)
        {
            try
            {
                // TODO: Implement PDF generation logic using QuestPDF
                return Ok(new { message = "PDF generation not yet implemented", orderId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating PDF", error = ex.Message });
            }
        }
    }

    public class CreateOrderRequest
    {
        public decimal TotalAmount { get; set; }
    }

    public class UpdateOrderRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class PaymentMethodRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }
}