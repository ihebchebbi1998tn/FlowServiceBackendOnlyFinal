using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("notification/api/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving notifications", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _notificationService.DeleteAsync(id);
                return Ok(new { message = "Notification deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting notification", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> MarkRead(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error marking notification as read", error = ex.Message });
            }
        }
    }
}