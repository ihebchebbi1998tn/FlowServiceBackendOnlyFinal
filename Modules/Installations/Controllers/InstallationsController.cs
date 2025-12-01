using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Modules.Installations.DTOs;
using MyApi.Modules.Installations.Services;
using System.Security.Claims;

namespace MyApi.Modules.Installations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/installations")]
    public class InstallationsController : ControllerBase
    {
        private readonly IInstallationService _installationService;
        private readonly ILogger<InstallationsController> _logger;

        public InstallationsController(IInstallationService installationService, ILogger<InstallationsController> logger)
        {
            _installationService = installationService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        }

        /// <summary>
        /// Get all installations with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInstallations(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] string? contact_id = null,
            [FromQuery] string? tags = null,
            [FromQuery] bool? has_warranty = null,
            [FromQuery] string? maintenance_frequency = null,
            [FromQuery] DateTime? created_from = null,
            [FromQuery] DateTime? created_to = null,
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 20,
            [FromQuery] string sort_by = "created_at",
            [FromQuery] string sort_order = "desc"
        )
        {
            try
            {
                var result = await _installationService.GetInstallationsAsync(
                    search, category, status, contact_id, tags, has_warranty,
                    maintenance_frequency, created_from, created_to,
                    page, page_size, sort_by, sort_order
                );

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching installations");
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while fetching installations"
                    }
                });
            }
        }

        /// <summary>
        /// Get single installation by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstallationById(string id)
        {
            try
            {
                var installation = await _installationService.GetInstallationByIdAsync(id);
                if (installation == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = new
                        {
                            code = "INSTALLATION_NOT_FOUND",
                            message = "Installation not found"
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = installation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching installation {InstallationId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while fetching the installation"
                    }
                });
            }
        }

        /// <summary>
        /// Create a new installation
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateInstallation([FromBody] CreateInstallationDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var installation = await _installationService.CreateInstallationAsync(createDto, userId);

                return CreatedAtAction(nameof(GetInstallationById), new { id = installation.Id }, new
                {
                    success = true,
                    data = installation
                });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = ex.Message
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating installation");
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while creating the installation"
                    }
                });
            }
        }

        /// <summary>
        /// Update an installation
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInstallation(string id, [FromBody] UpdateInstallationDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var installation = await _installationService.UpdateInstallationAsync(id, updateDto, userId);

                return Ok(new
                {
                    success = true,
                    data = installation
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    error = new
                    {
                        code = "INSTALLATION_NOT_FOUND",
                        message = "Installation not found"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating installation {InstallationId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while updating the installation"
                    }
                });
            }
        }

        /// <summary>
        /// Delete an installation
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstallation(string id)
        {
            try
            {
                var result = await _installationService.DeleteInstallationAsync(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = new
                        {
                            code = "INSTALLATION_NOT_FOUND",
                            message = "Installation not found"
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Installation deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting installation {InstallationId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while deleting the installation"
                    }
                });
            }
        }

        /// <summary>
        /// Get maintenance history for an installation
        /// </summary>
        [HttpGet("{id}/maintenance-history")]
        public async Task<IActionResult> GetMaintenanceHistory(
            string id,
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 20
        )
        {
            try
            {
                var histories = await _installationService.GetMaintenanceHistoryAsync(id, page, page_size);
                return Ok(new
                {
                    success = true,
                    data = histories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching maintenance history for installation {InstallationId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while fetching maintenance history"
                    }
                });
            }
        }

        /// <summary>
        /// Add maintenance history record
        /// </summary>
        [HttpPost("{id}/maintenance-history")]
        public async Task<IActionResult> AddMaintenanceHistory(string id, [FromBody] MaintenanceHistoryDto historyDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var history = await _installationService.AddMaintenanceHistoryAsync(id, historyDto, userId);

                return CreatedAtAction(nameof(GetInstallationById), new { id }, new
                {
                    success = true,
                    data = history
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    error = new
                    {
                        code = "INSTALLATION_NOT_FOUND",
                        message = "Installation not found"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding maintenance history for installation {InstallationId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An error occurred while adding maintenance history"
                    }
                });
            }
        }
    }
}
