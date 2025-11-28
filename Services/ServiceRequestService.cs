using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class ServiceRequestService
    {
        private readonly ConsolidatedDbContext _context;

        public ServiceRequestService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceRequest>> GetByUserIdAsync(string userId, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var query = _context.ServiceRequests
                .Include(sr => sr.Device)
                .Where(sr => sr.UserId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(sr => sr.Description.Contains(searchTerm) || sr.Status.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(sr => sr.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountByUserIdAsync(string userId, string? searchTerm = null)
        {
            var query = _context.ServiceRequests.Where(sr => sr.UserId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(sr => sr.Description.Contains(searchTerm) || sr.Status.Contains(searchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<ServiceRequest?> GetByIdAsync(string id)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Device)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
            return serviceRequest;
        }

        public async Task<ServiceRequest> UpdateAsync(ServiceRequest serviceRequest)
        {
            serviceRequest.UpdatedAt = DateTime.UtcNow;
            _context.ServiceRequests.Update(serviceRequest);
            await _context.SaveChangesAsync();
            return serviceRequest;
        }

        public async Task DeleteAsync(string id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
            {
                _context.ServiceRequests.Remove(serviceRequest);
                await _context.SaveChangesAsync();
            }
        }
    }
}