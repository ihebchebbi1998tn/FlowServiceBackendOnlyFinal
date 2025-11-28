using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class DeviceService
    {
        private readonly ConsolidatedDbContext _context;

        public DeviceService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<Device>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Devices.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Name.Contains(searchTerm) || d.Model.Contains(searchTerm));
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null)
        {
            var query = _context.Devices.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Name.Contains(searchTerm) || d.Model.Contains(searchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<Device?> GetByIdAsync(string id)
        {
            return await _context.Devices.FindAsync(id);
        }

        public async Task<Device> CreateAsync(Device device)
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<Device> UpdateAsync(Device device)
        {
            device.UpdatedAt = DateTime.UtcNow;
            _context.Devices.Update(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task DeleteAsync(string id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
            }
        }
    }
}