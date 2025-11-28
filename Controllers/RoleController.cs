using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("user/api/role")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var allRoles = await _roleService.GetAllAsync();
                
                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    allRoles = allRoles.Where(r => 
                        r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        r.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (r.Description != null && r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                var totalCount = allRoles.Count;
                var paginatedRoles = allRoles
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new { 
                    totalCount, 
                    pageNumber, 
                    pageSize, 
                    roles = paginatedRoles 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving roles", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var role = await _roleService.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving role", error = ex.Message });
            }
        }

        [HttpGet("ByUserId/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            try
            {
                // This would need UserService to get user's role
                // For now, return placeholder
                return Ok(new { message = "User role found", userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving user role", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest dto)
        {
            try
            {
                var role = new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    NormalizedName = dto.Name.ToUpper(),
                    DisplayName = dto.DisplayName,
                    Description = dto.Description,
                    Permissions = dto.Permissions,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdRole = await _roleService.CreateAsync(role);
                return Ok(new { message = "Role created successfully", role = createdRole });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating role", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleRequest dto)
        {
            try
            {
                var existingRole = await _roleService.GetByIdAsync(id);
                if (existingRole == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                existingRole.Name = dto.Name;
                existingRole.NormalizedName = dto.Name.ToUpper();
                existingRole.DisplayName = dto.DisplayName;
                existingRole.Description = dto.Description;
                existingRole.Permissions = dto.Permissions;
                existingRole.UpdatedAt = DateTime.UtcNow;

                var updatedRole = await _roleService.UpdateAsync(existingRole);
                return Ok(new { message = "Role updated successfully", role = updatedRole });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating role", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var role = await _roleService.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                await _roleService.DeleteAsync(id);
                return Ok(new { message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting role", error = ex.Message });
            }
        }
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}