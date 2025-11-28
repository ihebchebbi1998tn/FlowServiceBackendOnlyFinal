using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;
using Microsoft.AspNetCore.Identity;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("user/api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleService _roleService;

        public UserController(UserService userService, UserManager<ApplicationUser> userManager, RoleService roleService)
        {
            _userService = userService;
            _userManager = userManager;
            _roleService = roleService;
        }

        [HttpPost("add_list_of_users")]
        public async Task<IActionResult> AddListOfUsers([FromBody] List<CreateUserRequest> users)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var createdUsers = new List<object>();
                foreach (var userDto in users)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userDto.Email,
                        Email = userDto.Email,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        PhoneNumber = userDto.PhoneNumber,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, userDto.Password);
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(userDto.RoleId))
                        {
                            var role = await _roleService.GetByIdAsync(userDto.RoleId);
                            if (role != null)
                            {
                                await _userManager.AddToRoleAsync(user, role.Name);
                            }
                        }
                        createdUsers.Add(new { id = user.Id, email = user.Email, name = $"{user.FirstName} {user.LastName}" });
                    }
                }

                return Ok(new { message = "Users were added successfully", users = createdUsers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating users", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] CreateUserRequest dto)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RoleId = dto.RoleId
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Failed to create user", errors = result.Errors });
                }

                if (!string.IsNullOrEmpty(dto.RoleId))
                {
                    var role = await _roleService.GetByIdAsync(dto.RoleId);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                }

                return Ok(new { message = "User created successfully", user = new { id = user.Id, email = user.Email } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating user", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                
                return Ok(new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    roleId = user.RoleId,
                    roles = roles,
                    isActive = !user.IsArchived,
                    createdAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving user", error = ex.Message });
            }
        }

        [HttpPost("update_image")]
        public async Task<IActionResult> UpdateUserImage([FromForm] IFormFile image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                // TODO: Implement file upload logic with MinioService
                return Ok(new { message = "User image updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating user image", error = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var allUsers = await _userService.GetAllAsync();
                
                // Filter out archived users and apply search
                var activeUsers = allUsers.Where(u => !u.IsArchived);
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    activeUsers = activeUsers.Where(u => 
                        u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    );
                }

                var totalCount = activeUsers.Count();
                var paginatedUsers = activeUsers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        id = u.Id,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        roleId = u.RoleId,
                        createdAt = u.CreatedAt
                    })
                    .ToList();

                // Get unique company names (if this field exists in your model)
                var companyNames = new string[] { };

                return Ok(new { totalCount, pageNumber, pageSize, users = paginatedUsers, companyNames });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving users", error = ex.Message });
            }
        }

        [HttpGet("deleted")]
        public async Task<IActionResult> GetAllDeletedUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var allUsers = await _userService.GetAllAsync();
                var deletedUsers = allUsers.Where(u => u.IsArchived);

                var totalCount = deletedUsers.Count();
                var paginatedUsers = deletedUsers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        id = u.Id,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        roleId = u.RoleId,
                        createdAt = u.CreatedAt
                    })
                    .ToList();

                return Ok(new { totalCount, pageNumber, pageSize, users = paginatedUsers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving deleted users", error = ex.Message });
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest dto)
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.PhoneNumber = dto.PhoneNumber;
                user.RoleId = dto.RoleId;
                user.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateAsync(user);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating user", error = ex.Message });
            }
        }

        [HttpPut("roles/{userId}")]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] UpdateUserRolesRequest dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Remove existing roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new roles
                if (dto.RoleNames != null && dto.RoleNames.Any())
                {
                    await _userManager.AddToRolesAsync(user, dto.RoleNames);
                }

                // Update RoleId if provided
                if (!string.IsNullOrEmpty(dto.RoleId))
                {
                    user.RoleId = dto.RoleId;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new { message = "User roles updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating user roles", error = ex.Message });
            }
        }

        [HttpPut("{userId}/toggle-active")]
        public async Task<IActionResult> ToggleUserIsActive(string userId)
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.IsArchived = !user.IsArchived;
                user.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateAsync(user);
                return Ok(new { message = "User active status toggled successfully", isActive = !user.IsArchived });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error toggling user active status", error = ex.Message });
            }
        }

        [HttpPut("{userId}/toggle-delete")]
        public async Task<IActionResult> ToggleUserIsDeleted(string userId)
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.IsArchived = !user.IsArchived;
                user.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateAsync(user);
                return Ok(new { message = "User delete status toggled successfully", isDeleted = user.IsArchived });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error toggling user delete status", error = ex.Message });
            }
        }

        [HttpPost("language")]
        public async Task<IActionResult> ChangeUserLanguage([FromBody] ChangeLanguageRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Assuming there's a Language property on ApplicationUser
                // user.Language = dto.Language;
                user.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateAsync(user);
                return Ok(new { message = "User language updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating user language", error = ex.Message });
            }
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetUsersInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var users = await _userService.GetAllAsync();
                var userInfo = users.Where(u => !u.IsArchived).Select(u => new
                {
                    id = u.Id,
                    name = $"{u.FirstName} {u.LastName}",
                    email = u.Email
                }).ToList();

                return Ok(new { users = userInfo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving users info", error = ex.Message });
            }
        }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? RoleId { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? RoleId { get; set; }
    }

    public class UpdateUserRolesRequest
    {
        public List<string>? RoleNames { get; set; }
        public string? RoleId { get; set; }
    }

    public class ChangeLanguageRequest
    {
        public string Language { get; set; } = string.Empty;
    }
}