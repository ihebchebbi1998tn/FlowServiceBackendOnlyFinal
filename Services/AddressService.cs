using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class AddressService
    {
        private readonly ConsolidatedDbContext _context;

        public AddressService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<Address?> GetByUserIdAsync(string userId)
        {
            return await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Address> CreateAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAsync(Address address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task DeleteAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
        }
    }
}