using InsuranceFraudDetection.Core.Entities;

namespace InsuranceFraudDetection.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<int> GetNextAutoNumberAsync();
    }
}
