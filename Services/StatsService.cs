using ConsolidatedApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class StatsService
    {
        private readonly ConsolidatedDbContext _context;

        public StatsService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetUserStatsAsync(string userId)
        {
            var totalRequests = await _context.ServiceRequests
                .Where(sr => sr.UserId == userId)
                .CountAsync();

            var pendingRequests = await _context.ServiceRequests
                .Where(sr => sr.UserId == userId && sr.Status == "PENDING")
                .CountAsync();

            var completedRequests = await _context.ServiceRequests
                .Where(sr => sr.UserId == userId && sr.Status == "COMPLETED")
                .CountAsync();

            var totalOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .CountAsync();

            var revenue = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == "COMPLETED")
                .SumAsync(o => o.TotalAmount);

            return new
            {
                totalRequests,
                pendingRequests,
                completedRequests,
                totalOrders,
                revenue
            };
        }

        public async Task<object> GetGlobalStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalRequests = await _context.ServiceRequests.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalDevices = await _context.Devices.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "COMPLETED")
                .SumAsync(o => o.TotalAmount);

            return new
            {
                totalUsers,
                totalRequests,
                totalOrders,
                totalDevices,
                totalRevenue
            };
        }
    }
}