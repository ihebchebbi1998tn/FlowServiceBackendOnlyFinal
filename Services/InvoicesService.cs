using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidatedApi.Services
{
    public class InvoicesService
    {
        private readonly ConsolidatedDbContext _context;

        public InvoicesService(ConsolidatedDbContext context)
        {
            _context = context;
        }

        public async Task<List<Invoice>> GetByUserIdAsync(string userId)
        {
            return await _context.Invoices
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(string id)
        {
            return await _context.Invoices.FindAsync(id);
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            invoice.UpdatedAt = DateTime.UtcNow;
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task DeleteAsync(string id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }
        }
    }
}