using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class ClientOrganizationService
    {
        private readonly ConsolidatedDbContext _context;

        public ClientOrganizationService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClientOrganization>> GetAllAsync()
        {
            return await _context.ClientOrganizations
                .Include(co => co.Users)
                .Include(co => co.Roles)
                .Include(co => co.UiPages)
                .ToListAsync();
        }

        public async Task<ClientOrganization?> GetByIdAsync(string id)
        {
            return await _context.ClientOrganizations
                .Include(co => co.Users)
                .Include(co => co.Roles)
                .Include(co => co.UiPages)
                .FirstOrDefaultAsync(co => co.Id == id);
        }

        public async Task<ClientOrganization> CreateAsync(ClientOrganization organization)
        {
            _context.ClientOrganizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<ClientOrganization> UpdateAsync(ClientOrganization organization)
        {
            organization.UpdatedAt = DateTime.UtcNow;
            _context.ClientOrganizations.Update(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task DeleteAsync(string id)
        {
            var organization = await _context.ClientOrganizations.FindAsync(id);
            if (organization != null)
            {
                _context.ClientOrganizations.Remove(organization);
                await _context.SaveChangesAsync();
            }
        }
    }
}