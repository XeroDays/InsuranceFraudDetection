using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Core.Entities;
using InsuranceFraudDetection.Infrastructure.Data;
using InsuranceFraudDetection.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace InsuranceFraudDetection.Infrastructure.Persistence
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly InsuranceDbContext _context;
        private readonly ICustomLogger _logger;

        public ClaimRepository(InsuranceDbContext context, ICustomLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Claim> AddAsync(Claim claim)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Adding new claim to database: Type={claim.ClaimType}, Amount={claim.Amount.Amount}, UserId={claim.UserId}");
                
                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();
                
                await _logger.LogAsync(LogLevel.Information, $"Successfully added claim with ID: {claim.Id}");
                return claim;
            }
            catch (DbUpdateException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Database error while adding claim: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to save claim to database", ex);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Unexpected error while adding claim: {ex.Message}", ex);
                throw new InvalidOperationException("An error occurred while adding the claim", ex);
            }
        }

        public async Task<Claim> GetByIdAsync(int id)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Retrieving claim from database by ID: {id}");
                
                var claim = await _context.Claims
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (claim == null)
                {
                    await _logger.LogAsync(LogLevel.Warning, $"Claim with ID {id} not found in database");
                }
                else
                {
                    await _logger.LogAsync(LogLevel.Information, $"Successfully retrieved claim from database: ID={claim.Id}, Type={claim.ClaimType}");
                }

                return claim;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error retrieving claim by ID {id} from database: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while retrieving claim with ID {id}", ex);
            }
        }

        public async Task<IEnumerable<Claim>> GetAllAsync()
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, "Retrieving all claims from database");
                
                var claims = await _context.Claims
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                await _logger.LogAsync(LogLevel.Information, $"Successfully retrieved {claims.Count} claims from database");
                return claims;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error retrieving all claims from database: {ex.Message}", ex);
                throw new InvalidOperationException("An error occurred while retrieving all claims", ex);
            }
        }

        public async Task<Claim> UpdateAsync(Claim claim)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Updating claim in database: ID={claim.Id}, Type={claim.ClaimType}, Status={claim.Status}");
                
                _context.Claims.Update(claim);
                await _context.SaveChangesAsync();
                
                await _logger.LogAsync(LogLevel.Information, $"Successfully updated claim with ID: {claim.Id}");
                return claim;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Concurrency error while updating claim {claim.Id}: {ex.Message}", ex);
                throw new InvalidOperationException($"Claim with ID {claim.Id} was modified by another operation", ex);
            }
            catch (DbUpdateException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Database error while updating claim {claim.Id}: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to update claim in database", ex);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Unexpected error while updating claim {claim.Id}: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while updating claim with ID {claim.Id}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Attempting to delete claim from database: ID={id}");
                
                var claim = await _context.Claims.FindAsync(id);
                if (claim != null)
                {
                    _context.Claims.Remove(claim);
                    await _context.SaveChangesAsync();
                    await _logger.LogAsync(LogLevel.Information, $"Successfully deleted claim with ID: {id}");
                }
                else
                {
                    await _logger.LogAsync(LogLevel.Warning, $"Attempted to delete claim with ID {id}, but it was not found in database");
                }
            }
            catch (DbUpdateException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Database error while deleting claim {id}: {ex.Message}", ex);
                throw new InvalidOperationException($"Failed to delete claim with ID {id} from database", ex);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Unexpected error while deleting claim {id}: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while deleting claim with ID {id}", ex);
            }
        }
    }
}
