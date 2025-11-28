using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class InstallationRequestService
    {
        private readonly ConsolidatedDbContext _context;

        public InstallationRequestService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<InstallationRequest>> GetByUserIdAsync(string userId)
        {
            return await _context.InstallationRequests
                .Where(ir => ir.UserId == userId)
                .OrderByDescending(ir => ir.CreatedAt)
                .ToListAsync();
        }

        public async Task<InstallationRequest?> GetByIdAsync(string id)
        {
            return await _context.InstallationRequests.FindAsync(id);
        }

        public async Task<InstallationRequest> CreateAsync(InstallationRequest request)
        {
            _context.InstallationRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<InstallationRequest> UpdateAsync(InstallationRequest request)
        {
            request.UpdatedAt = DateTime.UtcNow;
            _context.InstallationRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task DeleteAsync(string id)
        {
            var request = await _context.InstallationRequests.FindAsync(id);
            if (request != null)
            {
                _context.InstallationRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }
    }
}