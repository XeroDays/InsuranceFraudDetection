using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Core.Entities;
using InsuranceFraudDetection.Infrastructure.Data;
using InsuranceFraudDetection.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace InsuranceFraudDetection.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly InsuranceDbContext _context;
        private readonly ICustomLogger _logger;

        public UserRepository(InsuranceDbContext context, ICustomLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> AddAsync(User user)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Adding new user to database: Name={user.FullName}, Email={user.Email}");
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                await _logger.LogAsync(LogLevel.Information, $"Successfully added user with ID: {user.Id}");
                return user;
            }
            catch (DbUpdateException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Database error while adding user: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to save user to database", ex);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Unexpected error while adding user: {ex.Message}", ex);
                throw new InvalidOperationException("An error occurred while adding the user", ex);
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Retrieving user from database by ID: {id}");
                
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    await _logger.LogAsync(LogLevel.Warning, $"User with ID {id} not found in database");
                }
                else
                {
                    await _logger.LogAsync(LogLevel.Information, $"Successfully retrieved user from database: ID={user.Id}, Name={user.FullName}");
                }

                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error retrieving user by ID {id} from database: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while retrieving user with ID {id}", ex);
            }
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Retrieving user from database by email: {email}");
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    await _logger.LogAsync(LogLevel.Warning, $"User with email {email} not found in database");
                }
                else
                {
                    await _logger.LogAsync(LogLevel.Information, $"Successfully retrieved user from database by email: ID={user.Id}, Name={user.FullName}");
                }

                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error retrieving user by email {email} from database: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while retrieving user with email {email}", ex);
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, "Retrieving all users from database");
                
                var users = await _context.Users.ToListAsync();

                await _logger.LogAsync(LogLevel.Information, $"Successfully retrieved {users.Count} users from database");
                return users;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error retrieving all users from database: {ex.Message}", ex);
                throw new InvalidOperationException("An error occurred while retrieving all users", ex);
            }
        }

        public async Task<User> UpdateAsync(User user)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Updating user in database: ID={user.Id}, Name={user.FullName}, Email={user.Email}");
                
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                
                await _logger.LogAsync(LogLevel.Information, $"Successfully updated user with ID: {user.Id}");
                return user;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Concurrency error while updating user {user.Id}: {ex.Message}", ex);
                throw new InvalidOperationException($"User with ID {user.Id} was modified by another operation", ex);
            }
            catch (DbUpdateException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Database error while updating user {user.Id}: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to update user in database", ex);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Unexpected error while updating user {user.Id}: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while updating user with ID {user.Id}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Attempting to delete user from database: ID={id}");
                
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    await _logger.LogAsync(LogLevel.Information, $"Successfully deleted user with ID: {id}");
                }
                else
                {
                    await _logger.LogAsync(LogLevel.Warning, $"Attempted to delete user with ID {id}, but it was not found in database");
                }
            }
            catch (DbUpdateException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Database error while deleting user {id}: {ex.Message}", ex);
                throw new InvalidOperationException($"Failed to delete user with ID {id} from database", ex);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Unexpected error while deleting user {id}: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while deleting user with ID {id}", ex);
            }
        }

        public async Task<int> GetNextAutoNumberAsync()
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, "Calculating next auto number for user");
                
                var maxId = await _context.Users.MaxAsync(u => (int?)u.Id) ?? 0;
                var nextId = maxId + 1;
                
                await _logger.LogAsync(LogLevel.Information, $"Next auto number calculated: {nextId} (previous max: {maxId})");
                return nextId;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error calculating next auto number for user: {ex.Message}", ex);
                throw new InvalidOperationException("An error occurred while calculating the next auto number", ex);
            }
        }
    }
}
