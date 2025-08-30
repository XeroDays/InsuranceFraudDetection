using InsuranceFraudDetection.Core.Entities;

namespace InsuranceFraudDetection.Application.Interfaces
{
    public interface IClaimRepository
    {
        Task<Claim> AddAsync(Claim claim);
        Task<Claim> GetByIdAsync(int id);
        Task<IEnumerable<Claim>> GetAllAsync();
        Task<Claim> UpdateAsync(Claim claim);
        Task DeleteAsync(int id);
    }
}
