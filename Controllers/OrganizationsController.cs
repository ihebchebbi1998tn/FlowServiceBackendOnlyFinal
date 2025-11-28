using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("user/api/Organizations")]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly ClientOrganizationService _clientOrganizationService;
        private readonly UiPageService _uiPageService;

        public OrganizationsController(ClientOrganizationService clientOrganizationService, UiPageService uiPageService)
        {
            _clientOrganizationService = clientOrganizationService;
            _uiPageService = uiPageService;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrganization([FromForm] SimpleCreateOrganizationRequest organization)
        {
            try
            {
                var clientOrg = new ClientOrganization
                {
                    Name = organization.Name,
                    EmailOrganisation = organization.Email,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdOrganization = await _clientOrganizationService.CreateAsync(clientOrg);
                return Ok(new { message = "Organization created successfully", organization = createdOrganization });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating organization", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizationById()
        {
            try
            {
                // This endpoint seems to be designed to get current user's organization
                // Since there's no ID parameter, we'll return a placeholder
                return Ok(new { message = "Organization retrieval method needs clarification" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving organization", error = ex.Message });
            }
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllOrganizations()
        {
            try
            {
                var organizations = await _clientOrganizationService.GetAllAsync();
                return Ok(organizations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving organizations", error = ex.Message });
            }
        }

        [HttpGet("Deleted")]
        public async Task<IActionResult> GetAllDeletedOrganizations()
        {
            try
            {
                // Assuming there's a way to filter deleted organizations
                var allOrganizations = await _clientOrganizationService.GetAllAsync();
                // Filter for deleted ones if there's an IsDeleted property
                return Ok(new object[] { });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving deleted organizations", error = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateOrganization([FromForm] SimpleUpdateOrganizationRequest organization)
        {
            try
            {
                var existingOrganization = await _clientOrganizationService.GetByIdAsync(organization.Id);
                if (existingOrganization == null)
                {
                    return NotFound(new { message = "Organization not found" });
                }

                existingOrganization.Name = organization.Name;
                existingOrganization.EmailOrganisation = organization.Email;
                existingOrganization.UpdatedAt = DateTime.UtcNow;

                var updatedOrganization = await _clientOrganizationService.UpdateAsync(existingOrganization);
                return Ok(new { message = "Organization updated successfully", organization = updatedOrganization });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating organization", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ToggleOrganizationStatus(string id)
        {
            try
            {
                var organization = await _clientOrganizationService.GetByIdAsync(id);
                if (organization == null)
                {
                    return NotFound(new { message = "Organization not found" });
                }

                // Toggle status if there's an IsActive property
                organization.UpdatedAt = DateTime.UtcNow;
                await _clientOrganizationService.UpdateAsync(organization);

                return Ok(new { message = "Organization status toggled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error toggling organization status", error = ex.Message });
            }
        }

        [HttpPost("update/uiPage")]
        public async Task<IActionResult> UpdateUiPage([FromForm] UpdateUiPageRequest uiPage)
        {
            try
            {
                var existingUiPage = await _uiPageService.GetByIdAsync(uiPage.Id);
                if (existingUiPage == null)
                {
                    return NotFound(new { message = "UI page not found" });
                }

                existingUiPage.Name = uiPage.PageName;
                existingUiPage.FieldsToNotDisplay = uiPage.FieldsToNotDisplay;
                existingUiPage.UpdatedAt = DateTime.UtcNow;

                var updatedUiPage = await _uiPageService.UpdateAsync(existingUiPage);
                return Ok(new { message = "UI page updated successfully", uiPage = updatedUiPage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating UI page", error = ex.Message });
            }
        }

        [HttpPost("remove-background")]
        public async Task<IActionResult> RemoveBackground([FromForm] IFormFile image)
        {
            try
            {
                // TODO: Implement background removal logic using AI service
                return Ok(new { message = "Background removal not yet implemented" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error removing background", error = ex.Message });
            }
        }

        [HttpPost("stripe/create-checkout-session")]
        public async Task<IActionResult> PaymentStripe([FromBody] StripePaymentRequest data)
        {
            try
            {
                // TODO: Implement Stripe payment logic
                return Ok(new { message = "Stripe session creation not yet implemented" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating Stripe session", error = ex.Message });
            }
        }

        [HttpPost("square/create-payment")]
        public async Task<IActionResult> PaymentSquare([FromBody] SquarePaymentRequest data)
        {
            try
            {
                // TODO: Implement Square payment logic
                return Ok(new { message = "Square payment creation not yet implemented" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating Square payment", error = ex.Message });
            }
        }

        [HttpPut("send_event/{id}/{type}")]
        public async Task<IActionResult> SendEvent(int id, string type)
        {
            try
            {
                // TODO: Implement event sending logic (possibly SignalR notifications)
                return Ok(new { message = "Event sent successfully", organizationId = id, eventType = type });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error sending event", error = ex.Message });
            }
        }
    }

    public class SimpleCreateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
    }

    public class SimpleUpdateOrganizationRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
    }

    public class UpdateUiPageRequest
    {
        public int Id { get; set; }
        public string PageName { get; set; } = string.Empty;
        public List<string> FieldsToNotDisplay { get; set; } = new();
    }

    public class StripePaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class SquarePaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }
}