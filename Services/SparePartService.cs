using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class SparePartService
    {
        private readonly ConsolidatedDbContext _context;

        public SparePartService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<SparePart>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var query = _context.SpareParts.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(sp => sp.Name.Contains(searchTerm) || sp.PartNumber.Contains(searchTerm));
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null)
        {
            var query = _context.SpareParts.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(sp => sp.Name.Contains(searchTerm) || sp.PartNumber.Contains(searchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<SparePart?> GetByIdAsync(string id)
        {
            return await _context.SpareParts.FindAsync(id);
        }

        public async Task<SparePart> CreateAsync(SparePart sparePart)
        {
            _context.SpareParts.Add(sparePart);
            await _context.SaveChangesAsync();
            return sparePart;
        }

        public async Task<SparePart> UpdateAsync(SparePart sparePart)
        {
            sparePart.UpdatedAt = DateTime.UtcNow;
            _context.SpareParts.Update(sparePart);
            await _context.SaveChangesAsync();
            return sparePart;
        }

        public async Task DeleteAsync(string id)
        {
            var sparePart = await _context.SpareParts.FindAsync(id);
            if (sparePart != null)
            {
                _context.SpareParts.Remove(sparePart);
                await _context.SaveChangesAsync();
            }
        }
    }
}