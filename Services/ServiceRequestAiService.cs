using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class ServiceRequestAiService
    {
        private readonly ConsolidatedDbContext _context;
        private readonly GeminiPromptService _geminiService;

        public ServiceRequestAiService(ConsolidatedDbContext context, GeminiPromptService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        public async Task<List<ServiceRequest>> GetAiProcessedRequestsAsync(string? searchTerm = null, int companyId = 0, int page = 1, int pageSize = 10)
        {
            var query = _context.ServiceRequests
                .Include(sr => sr.Device)
                .Where(sr => sr.Status == "AI_PROCESSED" || sr.Status == "AI_PENDING");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(sr => sr.Description.Contains(searchTerm));
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetAiProcessedRequestsCountAsync(string? searchTerm = null, int companyId = 0)
        {
            var query = _context.ServiceRequests
                .Where(sr => sr.Status == "AI_PROCESSED" || sr.Status == "AI_PENDING");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(sr => sr.Description.Contains(searchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<string> ProcessServiceRequestWithAiAsync(string requestId)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(requestId);
            if (serviceRequest == null)
            {
                throw new ArgumentException("Service request not found");
            }

            var aiAnalysis = await _geminiService.AnalyzeServiceRequestAsync(serviceRequest.Description);
            
            serviceRequest.Status = "AI_PROCESSED";
            serviceRequest.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return aiAnalysis;
        }
    }
}