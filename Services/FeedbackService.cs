using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class FeedbackService
    {
        private readonly ConsolidatedDbContext _context;

        public FeedbackService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<Feedback>> GetByUserIdAsync(string userId)
        {
            return await _context.Feedbacks
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdAsync(string id)
        {
            return await _context.Feedbacks.FindAsync(id);
        }

        public async Task<Feedback> CreateAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<Feedback> UpdateAsync(Feedback feedback)
        {
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task DeleteAsync(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
            }
        }
    }
}