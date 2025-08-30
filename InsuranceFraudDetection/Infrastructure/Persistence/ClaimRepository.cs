using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Core.Entities;
using InsuranceFraudDetection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InsuranceFraudDetection.Infrastructure.Persistence
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly InsuranceDbContext _context;

        public ClaimRepository(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<Claim> AddAsync(Claim claim)
        {
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<Claim> GetByIdAsync(int id)
        {
            return await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Claim>> GetAllAsync()
        {
            return await _context.Claims
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Claim> UpdateAsync(Claim claim)
        {
            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task DeleteAsync(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();
            }
        }
    }
}
