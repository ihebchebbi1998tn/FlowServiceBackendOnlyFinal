using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("chat/api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var messages = await _chatService.GetMessagesByUserIdAsync(userId);
                
                // Group messages by session/organization for session-like behavior
                var sessions = messages.GroupBy(m => m.OrganizationId ?? "default")
                    .Select(g => new {
                        id = g.Key,
                        organizationId = g.Key,
                        messageCount = g.Count(),
                        lastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Content ?? "",
                        lastActivity = g.Max(m => m.Timestamp)
                    }).ToArray();

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving chat sessions", error = ex.Message });
            }
        }

        [HttpGet("messages/{sessionId}")]
        public async Task<IActionResult> GetMessages(string sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var messages = await _chatService.GetMessagesByUserIdAsync(userId);
                var sessionMessages = messages.Where(m => (m.OrganizationId ?? "default") == sessionId).ToArray();
                return Ok(sessionMessages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving messages", error = ex.Message });
            }
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var message = new ChatMessage
                {
                    Content = request.Content,
                    UserId = userId,
                    OrganizationId = request.OrganizationId,
                    SessionId = request.SessionId,
                    IsFromUser = true,
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                var createdMessage = await _chatService.CreateMessageAsync(message);
                return Ok(new { message = "Message sent successfully", chatMessage = createdMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error sending message", error = ex.Message });
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
                await _chatService.DeleteMessageAsync(id);
                return Ok(new { message = "Message deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting message", error = ex.Message });
            }
        }
    }

    public class ChatUpdateDto
    {
        public string Title { get; set; } = string.Empty;
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? OrganizationId { get; set; }
        public string? SessionId { get; set; }
    }
}