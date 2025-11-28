using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsolidatedApi.Services;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("service/api/Feedback")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateFeedbackRequest feedback)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var newFeedback = new Feedback
                {
                    Content = feedback.Content,
                    Rating = feedback.Rating,
                    ServiceRequestId = feedback.ServiceRequestId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdFeedback = await _feedbackService.CreateAsync(newFeedback);
                return Ok(new { message = "Feedback created successfully", feedback = createdFeedback });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating feedback", error = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateFeedbackRequest feedback)
        {
            try
            {
                var existingFeedback = await _feedbackService.GetByIdAsync(feedback.Id);
                if (existingFeedback == null)
                {
                    return NotFound(new { message = "Feedback not found" });
                }

                existingFeedback.Content = feedback.Content;
                existingFeedback.Rating = feedback.Rating;

                var updatedFeedback = await _feedbackService.UpdateAsync(existingFeedback);
                return Ok(new { message = "Feedback updated successfully", feedback = updatedFeedback });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating feedback", error = ex.Message });
            }
        }
    }

    public class CreateFeedbackRequest
    {
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? ServiceRequestId { get; set; }
    }

    public class UpdateFeedbackRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}