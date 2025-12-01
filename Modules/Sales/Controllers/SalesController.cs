using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Modules.Sales.DTOs;
using MyApi.Modules.Sales.Services;
using System.Security.Claims;

namespace MyApi.Modules.Sales.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly ILogger<SalesController> _logger;

        public SalesController(ISaleService saleService, ILogger<SalesController> logger)
        {
            _saleService = saleService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        }

        /// <summary>
        /// Get all sales with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSales(
            [FromQuery] string? status = null,
            [FromQuery] string? stage = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? contact_id = null,
            [FromQuery] DateTime? date_from = null,
            [FromQuery] DateTime? date_to = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string sort_by = "updated_at",
            [FromQuery] string sort_order = "desc"
        )
        {
            try
            {
                var result = await _saleService.GetSalesAsync(
                    status, stage, priority, contact_id,
                    date_from, date_to, search,
                    page, limit, sort_by, sort_order
                );

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sales");
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while fetching sales"
                    }
                });
            }
        }

        /// <summary>
        /// Get sale statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(
            [FromQuery] DateTime? date_from = null,
            [FromQuery] DateTime? date_to = null
        )
        {
            try
            {
                var stats = await _saleService.GetSaleStatsAsync(date_from, date_to);
                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sale statistics");
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while fetching statistics"
                    }
                });
            }
        }

        /// <summary>
        /// Get single sale by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSaleById(string id)
        {
            try
            {
                var sale = await _saleService.GetSaleByIdAsync(id);
                if (sale == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = new
                        {
                            code = "SALE_NOT_FOUND",
                            message = "Sale not found"
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = sale
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sale {SaleId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while fetching the sale"
                    }
                });
            }
        }

        /// <summary>
        /// Create a new sale
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var sale = await _saleService.CreateSaleAsync(createDto, userId);

                return CreatedAtAction(nameof(GetSaleById), new { id = sale.Id }, new
                {
                    success = true,
                    data = sale
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sale");
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while creating the sale"
                    }
                });
            }
        }

        /// <summary>
        /// Create a sale from an offer (conversion)
        /// </summary>
        [HttpPost("from-offer/{offerId}")]
        public async Task<IActionResult> CreateSaleFromOffer(string offerId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var sale = await _saleService.CreateSaleFromOfferAsync(offerId, userId);

                return CreatedAtAction(nameof(GetSaleById), new { id = sale.Id }, new
                {
                    success = true,
                    data = sale
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    error = new
                    {
                        code = "OFFER_NOT_FOUND",
                        message = ex.Message
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sale from offer");
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while creating the sale from offer"
                    }
                });
            }
        }

        /// <summary>
        /// Update a sale
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateSale(string id, [FromBody] UpdateSaleDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var sale = await _saleService.UpdateSaleAsync(id, updateDto, userId);

                return Ok(new
                {
                    success = true,
                    data = sale
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    error = new
                    {
                        code = "SALE_NOT_FOUND",
                        message = "Sale not found"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sale {SaleId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while updating the sale"
                    }
                });
            }
        }

        /// <summary>
        /// Delete a sale
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(string id)
        {
            try
            {
                var result = await _saleService.DeleteSaleAsync(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = new
                        {
                            code = "SALE_NOT_FOUND",
                            message = "Sale not found"
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Sale deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sale {SaleId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while deleting the sale"
                    }
                });
            }
        }

        /// <summary>
        /// Get sale activities
        /// </summary>
        [HttpGet("{id}/activities")]
        public async Task<IActionResult> GetSaleActivities(
            string id,
            [FromQuery] string? type = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20
        )
        {
            try
            {
                var activities = await _saleService.GetSaleActivitiesAsync(id, type, page, limit);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        activities,
                        pagination = new
                        {
                            page,
                            limit,
                            total = activities.Count
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching activities for sale {SaleId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while fetching activities"
                    }
                });
            }
        }

        /// <summary>
        /// Add item to sale
        /// </summary>
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddSaleItem(string id, [FromBody] CreateSaleItemDto itemDto)
        {
            try
            {
                var item = await _saleService.AddSaleItemAsync(id, itemDto);
                return CreatedAtAction(nameof(GetSaleById), new { id }, new
                {
                    success = true,
                    data = item
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    error = new
                    {
                        code = "SALE_NOT_FOUND",
                        message = "Sale not found"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to sale {SaleId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while adding the item"
                    }
                });
            }
        }

        /// <summary>
        /// Update sale item
        /// </summary>
        [HttpPatch("{id}/items/{itemId}")]
        public async Task<IActionResult> UpdateSaleItem(string id, string itemId, [FromBody] CreateSaleItemDto itemDto)
        {
            try
            {
                var item = await _saleService.UpdateSaleItemAsync(id, itemId, itemDto);
                return Ok(new
                {
                    success = true,
                    data = item
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    error = new
                    {
                        code = "ITEM_NOT_FOUND",
                        message = ex.Message
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId} in sale {SaleId}", itemId, id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while updating the item"
                    }
                });
            }
        }

        /// <summary>
        /// Delete sale item
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> DeleteSaleItem(string id, string itemId)
        {
            try
            {
                var result = await _saleService.DeleteSaleItemAsync(id, itemId);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = new
                        {
                            code = "ITEM_NOT_FOUND",
                            message = "Item not found"
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Item deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {ItemId} from sale {SaleId}", itemId, id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while deleting the item"
                    }
                });
            }
        }
    }
}
