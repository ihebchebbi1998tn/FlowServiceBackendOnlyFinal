using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("user/api/ClientOrganization")]
    [Authorize]
    public class ClientOrganizationController : ControllerBase
    {
        private readonly ClientOrganizationService _organizationService;

        public ClientOrganizationController(ClientOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var organizations = await _organizationService.GetAllAsync();
                return Ok(organizations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving organizations", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var organization = await _organizationService.GetByIdAsync(id);
                if (organization == null)
                    return NotFound(new { message = "Organization not found" });
                
                return Ok(organization);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving organization", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateOrganizationRequest request)
        {
            try
            {
                var organization = new ClientOrganization
                {
                    Name = request.Name,
                    EmailOrganisation = request.EmailOrganisation,
                    PrimaryColor = request.PrimaryColor ?? "#1976d2",
                    SecondaryColor = request.SecondaryColor ?? "#dc004e",
                    LogoName = request.LogoName ?? "",
                    LogoUrl = request.LogoUrl ?? "",
                    Description = request.Description,
                    StreetName = request.StreetName,
                    City = request.City,
                    Country = request.Country,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    Longitude = request.Longitude,
                    Latitude = request.Latitude,
                    OnlinePaymentSolutions = request.OnlinePaymentSolutions,
                    IsBgRemoved = request.IsBgRemoved ?? false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdOrganization = await _organizationService.CreateAsync(organization);
                return Ok(new { message = "Organization created successfully", organization = createdOrganization });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating organization", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateOrganizationRequest request)
        {
            try
            {
                var existingOrganization = await _organizationService.GetByIdAsync(id);
                if (existingOrganization == null)
                    return NotFound(new { message = "Organization not found" });

                existingOrganization.Name = request.Name;
                existingOrganization.EmailOrganisation = request.EmailOrganisation;
                existingOrganization.PrimaryColor = request.PrimaryColor ?? existingOrganization.PrimaryColor;
                existingOrganization.SecondaryColor = request.SecondaryColor ?? existingOrganization.SecondaryColor;
                existingOrganization.LogoName = request.LogoName ?? existingOrganization.LogoName;
                existingOrganization.LogoUrl = request.LogoUrl ?? existingOrganization.LogoUrl;
                existingOrganization.Description = request.Description ?? existingOrganization.Description;
                existingOrganization.StreetName = request.StreetName ?? existingOrganization.StreetName;
                existingOrganization.City = request.City ?? existingOrganization.City;
                existingOrganization.Country = request.Country ?? existingOrganization.Country;
                existingOrganization.State = request.State ?? existingOrganization.State;
                existingOrganization.ZipCode = request.ZipCode ?? existingOrganization.ZipCode;
                existingOrganization.Longitude = request.Longitude ?? existingOrganization.Longitude;
                existingOrganization.Latitude = request.Latitude ?? existingOrganization.Latitude;
                existingOrganization.OnlinePaymentSolutions = request.OnlinePaymentSolutions ?? existingOrganization.OnlinePaymentSolutions;
                existingOrganization.IsBgRemoved = request.IsBgRemoved ?? existingOrganization.IsBgRemoved;
                existingOrganization.UpdatedAt = DateTime.UtcNow;

                var updatedOrganization = await _organizationService.UpdateAsync(existingOrganization);
                return Ok(new { message = "Organization updated successfully", organization = updatedOrganization });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating organization", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var organization = await _organizationService.GetByIdAsync(id);
                if (organization == null)
                    return NotFound(new { message = "Organization not found" });

                await _organizationService.DeleteAsync(id);
                return Ok(new { message = "Organization deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting organization", error = ex.Message });
            }
        }
    }

    public class CreateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string EmailOrganisation { get; set; } = string.Empty;
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? LogoName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? StreetName { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string? OnlinePaymentSolutions { get; set; }
        public bool? IsBgRemoved { get; set; }
    }

    public class UpdateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string EmailOrganisation { get; set; } = string.Empty;
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? LogoName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? StreetName { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string? OnlinePaymentSolutions { get; set; }
        public bool? IsBgRemoved { get; set; }
    }
}