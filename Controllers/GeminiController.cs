using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("user/api/gemini")]
    [Authorize]
    public class GeminiController : ControllerBase
    {
        private readonly GeminiPromptService _geminiService;
        private readonly UserService _userService;
        private readonly DeviceService _deviceService;
        private readonly SparePartService _sparePartService;

        public GeminiController(
            GeminiPromptService geminiService,
            UserService userService,
            DeviceService deviceService,
            SparePartService sparePartService)
        {
            _geminiService = geminiService;
            _userService = userService;
            _deviceService = deviceService;
            _sparePartService = sparePartService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadUsers([FromForm] GeminiUploadRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var users = await _userService.GetAllAsync();
                var usersData = users.Select(u => new
                {
                    id = u.Id,
                    name = $"{u.FirstName} {u.LastName}",
                    email = u.Email,
                    role = u.RoleId,
                    organization = u.OrganizationId,
                    createdAt = u.CreatedAt,
                    isActive = !u.IsArchived
                });

                var prompt = $"Analyze these users data: {System.Text.Json.JsonSerializer.Serialize(usersData)}. {request.Prompt ?? "Provide insights and recommendations."}";
                var response = await _geminiService.GenerateResponseAsync(prompt);

                return Ok(new { 
                    message = "Users data analyzed by Gemini", 
                    analysis = response,
                    userCount = users.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error analyzing users with Gemini", error = ex.Message });
            }
        }

        [HttpPost("upload_devices")]
        public async Task<IActionResult> UploadDevices([FromForm] GeminiUploadRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Get user's organization for filtering
                var user = await _userService.GetByIdAsync(userId);
                if (user == null) return Unauthorized();

                var devices = await _deviceService.GetAllAsync(null, 1, int.MaxValue);
                var devicesData = devices.Where(d => d.OrganizationId == (user.OrganizationId ?? ""))
                    .Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        description = d.Description,
                        model = d.Model,
                        reference = d.Reference,
                        price = d.Price,
                        tva = d.Tva,
                        organizationName = d.OrganizationName,
                        createdAt = d.CreatedAt
                    });

                var prompt = $"Analyze these devices data: {System.Text.Json.JsonSerializer.Serialize(devicesData)}. {request.Prompt ?? "Provide insights about pricing, models, and inventory recommendations."}";
                var response = await _geminiService.GenerateResponseAsync(prompt);

                return Ok(new { 
                    message = "Devices data analyzed by Gemini", 
                    analysis = response,
                    deviceCount = devices.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error analyzing devices with Gemini", error = ex.Message });
            }
        }

        [HttpPost("upload_spare_parts")]
        public async Task<IActionResult> UploadSpareParts([FromForm] GeminiUploadRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Get user's organization for filtering
                var user = await _userService.GetByIdAsync(userId);
                if (user == null) return Unauthorized();

                var spareParts = await _sparePartService.GetAllAsync(null, 1, int.MaxValue);
                var sparePartsData = spareParts.Where(sp => sp.OrganizationId == (user.OrganizationId ?? ""))
                    .Select(sp => new
                    {
                        id = sp.Id,
                        title = sp.Title,
                        name = sp.Name,
                        description = sp.Description,
                        price = sp.Price,
                        partNumber = sp.PartNumber,
                        category = sp.Category,
                        stockQuantity = sp.StockQuantity,
                        organizationId = sp.OrganizationId,
                        createdAt = sp.CreatedAt
                    });

                var prompt = $"Analyze these spare parts data: {System.Text.Json.JsonSerializer.Serialize(sparePartsData)}. {request.Prompt ?? "Provide insights about inventory levels, pricing optimization, and restocking recommendations."}";
                var response = await _geminiService.GenerateResponseAsync(prompt);

                return Ok(new { 
                    message = "Spare parts data analyzed by Gemini", 
                    analysis = response,
                    sparePartsCount = spareParts.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error analyzing spare parts with Gemini", error = ex.Message });
            }
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] GeminiQuestionRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var response = await _geminiService.GenerateResponseAsync(request.Question);
                return Ok(new { 
                    message = "Question answered by Gemini", 
                    answer = response 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error getting answer from Gemini", error = ex.Message });
            }
        }
    }

    public class GeminiUploadRequest
    {
        public string? Prompt { get; set; }
    }

    public class GeminiQuestionRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}