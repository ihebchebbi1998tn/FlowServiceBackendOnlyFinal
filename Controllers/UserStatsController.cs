using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("user/api/stats")]
    [Authorize]
    public class UserStatsController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly ClientOrganizationService _organizationService;

        public UserStatsController(UserService userService, RoleService roleService, ClientOrganizationService organizationService)
        {
            _userService = userService;
            _roleService = roleService;
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var allUsers = await _userService.GetAllAsync();
                var allRoles = await _roleService.GetAllAsync();
                var allOrganizations = await _organizationService.GetAllAsync();

                // Calculate active users (users who are not archived)
                var activeUsers = allUsers.Where(u => !u.IsArchived).Count();
                var archivedUsers = allUsers.Where(u => u.IsArchived).Count();

                // Get organization breakdown
                var organizationStats = allOrganizations.Select(org => new
                {
                    id = org.Id,
                    name = org.Name,
                    userCount = allUsers.Count(u => u.OrganizationId == org.Id),
                    createdAt = org.CreatedAt
                });

                // Get role breakdown
                var roleStats = allRoles.Select(role => new
                {
                    id = role.Id,
                    name = role.Name,
                    displayName = role.DisplayName,
                    userCount = allUsers.Count(u => u.RoleId == role.Id),
                    organizationId = role.OrganizationId
                });

                return Ok(new { 
                    totalUsers = allUsers.Count,
                    activeUsers = activeUsers,
                    archivedUsers = archivedUsers,
                    totalOrganizations = allOrganizations.Count,
                    totalRoles = allRoles.Count,
                    systemHealth = "Good",
                    organizationStats = organizationStats,
                    roleStats = roleStats,
                    userGrowth = new
                    {
                        thisMonth = allUsers.Count(u => u.CreatedAt.Month == DateTime.UtcNow.Month && u.CreatedAt.Year == DateTime.UtcNow.Year),
                        lastMonth = allUsers.Count(u => u.CreatedAt.Month == DateTime.UtcNow.AddMonths(-1).Month && u.CreatedAt.Year == DateTime.UtcNow.AddMonths(-1).Year)
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving user stats", error = ex.Message });
            }
        }
    }
}