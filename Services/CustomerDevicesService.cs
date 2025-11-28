using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class CustomerDevicesService
    {
        private readonly ConsolidatedDbContext _context;

        public CustomerDevicesService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerDevice>> GetByUserIdAsync(string userId)
        {
            return await _context.CustomerDevices
                .Include(cd => cd.Device)
                .Where(cd => cd.UserId == userId)
                .ToListAsync();
        }

        public async Task<CustomerDevice?> GetByIdAsync(string id)
        {
            return await _context.CustomerDevices
                .Include(cd => cd.Device)
                .FirstOrDefaultAsync(cd => cd.Id == id);
        }

        public async Task<CustomerDevice> CreateAsync(CustomerDevice customerDevice)
        {
            _context.CustomerDevices.Add(customerDevice);
            await _context.SaveChangesAsync();
            return customerDevice;
        }

        public async Task<CustomerDevice> UpdateAsync(CustomerDevice customerDevice)
        {
            customerDevice.UpdatedAt = DateTime.UtcNow;
            _context.CustomerDevices.Update(customerDevice);
            await _context.SaveChangesAsync();
            return customerDevice;
        }

        public async Task DeleteAsync(string id)
        {
            var customerDevice = await _context.CustomerDevices.FindAsync(id);
            if (customerDevice != null)
            {
                _context.CustomerDevices.Remove(customerDevice);
                await _context.SaveChangesAsync();
            }
        }
    }
}