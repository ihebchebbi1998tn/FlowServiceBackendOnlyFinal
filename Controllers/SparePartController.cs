using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("service/api/SparePart")]
    [Authorize]
    public class SparePartController : ControllerBase
    {
        private readonly SparePartService _sparePartService;

        public SparePartController(SparePartService sparePartService)
        {
            _sparePartService = sparePartService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSparePartRequest dto)
        {
            try
            {
                var sparePart = new SparePart
                {
                    Title = dto.Title,
                    Name = dto.Name,
                    PartNumber = dto.PartNumber,
                    Description = dto.Description ?? string.Empty,
                    Price = dto.Price,
                    StockQuantity = dto.StockQuantity,
                    OrganizationId = dto.OrganizationId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdSparePart = await _sparePartService.CreateAsync(sparePart);
                return Ok(new { message = "Spare part created successfully", sparePart = createdSparePart });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating spare part", error = ex.Message });
            }
        }

        [HttpPost("add_list_of_spareParts")]
        public async Task<IActionResult> AddListSpareParts([FromForm] List<CreateSparePartRequest> spareParts)
        {
            try
            {
                var createdSpareParts = new List<SparePart>();
                foreach (var sparePartDto in spareParts)
                {
                    var sparePart = new SparePart
                    {
                        Title = sparePartDto.Title,
                        Name = sparePartDto.Name,
                        PartNumber = sparePartDto.PartNumber,
                        Description = sparePartDto.Description ?? string.Empty,
                        Price = sparePartDto.Price,
                        StockQuantity = sparePartDto.StockQuantity,
                        OrganizationId = sparePartDto.OrganizationId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var createdSparePart = await _sparePartService.CreateAsync(sparePart);
                    createdSpareParts.Add(createdSparePart);
                }

                return Ok(new { message = "Spare parts added successfully", spareParts = createdSpareParts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding spare parts", error = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateSparePartRequest sparePart)
        {
            try
            {
                var existingSparePart = await _sparePartService.GetByIdAsync(sparePart.Id);
                if (existingSparePart == null)
                {
                    return NotFound(new { message = "Spare part not found" });
                }

                existingSparePart.Title = sparePart.Title;
                existingSparePart.Name = sparePart.Name;
                existingSparePart.PartNumber = sparePart.PartNumber;
                existingSparePart.Description = sparePart.Description ?? existingSparePart.Description;
                existingSparePart.Price = sparePart.Price;
                existingSparePart.StockQuantity = sparePart.StockQuantity;
                existingSparePart.UpdatedAt = DateTime.UtcNow;

                var updatedSparePart = await _sparePartService.UpdateAsync(existingSparePart);
                return Ok(new { message = "Spare part updated successfully", sparePart = updatedSparePart });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating spare part", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm, [FromQuery] string? organizationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var allSpareParts = await _sparePartService.GetAllAsync();

                // Filter by organization if specified
                if (!string.IsNullOrWhiteSpace(organizationId))
                {
                    allSpareParts = allSpareParts.Where(sp => sp.OrganizationId == organizationId).ToList();
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    allSpareParts = allSpareParts.Where(sp => 
                        sp.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (sp.PartNumber != null && sp.PartNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (sp.Description != null && sp.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                var totalCount = allSpareParts.Count;
                var paginatedSpareParts = allSpareParts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new { totalCount, page, pageSize, data = paginatedSpareParts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving spare parts", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var sparePart = await _sparePartService.GetByIdAsync(id);
                if (sparePart == null)
                {
                    return NotFound(new { message = "Spare part not found" });
                }

                await _sparePartService.DeleteAsync(id);
                return Ok(new { message = "Spare part deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting spare part", error = ex.Message });
            }
        }
    }

    public class CreateSparePartRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string OrganizationId { get; set; } = string.Empty;
    }

    public class UpdateSparePartRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}