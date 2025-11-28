using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("chat/api/chat/UserToUser")]
    [Authorize]
    public class UserToUserChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly MinioService _minioService;

        public UserToUserChatController(ChatService chatService, MinioService minioService)
        {
            _chatService = chatService;
            _minioService = minioService;
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var chats = await _chatService.GetUserToUserChatsByUserIdAsync(userId);
                
                var sessions = chats.Select(c => new {
                    id = c.Id,
                    user1Id = c.User1Id,
                    user2Id = c.User2Id,
                    createdAt = c.CreatedAt,
                    isDeleted = c.IsDeleted,
                    otherUserId = c.User1Id == userId ? c.User2Id : c.User1Id
                }).ToArray();

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving chat sessions", error = ex.Message });
            }
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var messages = await _chatService.GetUserToUserMessagesByChatIdAsync(chatId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving messages", error = ex.Message });
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartChat([FromBody] StartChatRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var chat = new UserToUserChat
                {
                    User1Id = userId,
                    User2Id = request.OtherUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var createdChat = await _chatService.CreateUserToUserChatAsync(chat);
                return Ok(new { message = "Chat started successfully", chat = createdChat });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error starting chat", error = ex.Message });
            }
        }

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(string chatId, [FromBody] SendUserMessageRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var message = new UserToUserMessage
                {
                    Content = request.Content,
                    SenderId = userId,
                    ChatId = chatId,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                var createdMessage = await _chatService.CreateUserToUserMessageAsync(message);
                return Ok(new { message = "Message sent successfully", userMessage = createdMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error sending message", error = ex.Message });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            try
            {
                var bucket = "uploads";
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var contentType = file.ContentType;

                using var stream = file.OpenReadStream();
                var storedPath = await _minioService.UploadFileAsync(bucket, fileName, stream, contentType);

                return Ok(new
                {
                    objectName = storedPath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error uploading file", error = ex.Message });
            }
        }

        [HttpPut("{chatId}/messages/{messageId}/read")]
        public async Task<IActionResult> MarkAsRead(string chatId, string messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _chatService.MarkUserToUserMessageAsReadAsync(messageId, userId);
                return Ok(new { message = "Message marked as read" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error marking message as read", error = ex.Message });
            }
        }
    }

    public class StartChatRequest
    {
        public string OtherUserId { get; set; } = string.Empty;
    }

    public class SendUserMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}