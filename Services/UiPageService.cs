using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class UiPageService
    {
        private readonly ConsolidatedDbContext _context;

        public UiPageService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<UiPage>> GetByOrganizationIdAsync(string organizationId)
        {
            return await _context.UiPages
                .Where(up => up.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<UiPage?> GetByIdAsync(int id)
        {
            return await _context.UiPages.FindAsync(id);
        }

        public async Task<UiPage> CreateAsync(UiPage uiPage)
        {
            _context.UiPages.Add(uiPage);
            await _context.SaveChangesAsync();
            return uiPage;
        }

        public async Task<UiPage> UpdateAsync(UiPage uiPage)
        {
            uiPage.UpdatedAt = DateTime.UtcNow;
            _context.UiPages.Update(uiPage);
            await _context.SaveChangesAsync();
            return uiPage;
        }

        public async Task DeleteAsync(int id)
        {
            var uiPage = await _context.UiPages.FindAsync(id);
            if (uiPage != null)
            {
                _context.UiPages.Remove(uiPage);
                await _context.SaveChangesAsync();
            }
        }
    }
}