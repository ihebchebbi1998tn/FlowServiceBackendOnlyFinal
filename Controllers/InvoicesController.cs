using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("service/api/Invoices")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoicesService _invoicesService;

        public InvoicesController(InvoicesService invoicesService)
        {
            _invoicesService = invoicesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var invoices = await _invoicesService.GetByUserIdAsync(userId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving invoices", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest invoice)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var newInvoice = new Invoice
                {
                    UserId = userId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    IssuedDate = invoice.IssuedDate,
                    DueDate = invoice.DueDate,
                    TotalAmount = invoice.TotalAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdInvoice = await _invoicesService.CreateAsync(newInvoice);
                return Ok(new { message = "Invoice created successfully", invoice = createdInvoice });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating invoice", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateInvoiceRequest invoice)
        {
            try
            {
                var existingInvoice = await _invoicesService.GetByIdAsync(id);
                if (existingInvoice == null)
                {
                    return NotFound(new { message = "Invoice not found" });
                }

                existingInvoice.InvoiceNumber = invoice.InvoiceNumber;
                existingInvoice.TotalAmount = invoice.TotalAmount;
                existingInvoice.Status = invoice.Status;
                existingInvoice.DueDate = invoice.DueDate;
                existingInvoice.UpdatedAt = DateTime.UtcNow;

                var updatedInvoice = await _invoicesService.UpdateAsync(existingInvoice);
                return Ok(new { message = "Invoice updated successfully", invoice = updatedInvoice });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating invoice", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var invoice = await _invoicesService.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { message = "Invoice not found" });
                }

                await _invoicesService.DeleteAsync(id);
                return Ok(new { message = "Invoice deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting invoice", error = ex.Message });
            }
        }
    }

    public class CreateInvoiceRequest
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UpdateInvoiceRequest
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}