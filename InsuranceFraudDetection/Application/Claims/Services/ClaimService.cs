using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Models;
using InsuranceFraudDetection.Application.Claims.Commands;
using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Core.Entities;
using InsuranceFraudDetection.Core.ValueObjects;
using InsuranceFraudDetection.Infrastructure.Logging;

namespace InsuranceFraudDetection.Application.Claims.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICustomLogger _logger;

        public ClaimService(IClaimRepository claimRepository, IUserRepository userRepository, ICustomLogger logger)
        {
            _claimRepository = claimRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ClaimViewModel> SubmitClaimAsync(SubmitClaimCommand command)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Starting claim submission: Type={command.ClaimType}, Amount={command.Amount}, UserId={command.UserId}, DateFiled={command.DateFiled}");

                // Validation
                if (string.IsNullOrWhiteSpace(command.ClaimType))
                {
                    var errorMessage = "Claim type is required";
                    await _logger.LogAsync(LogLevel.Error, errorMessage);
                    throw new ArgumentException(errorMessage);
                }

                if (command.Amount <= 0)
                {
                    var errorMessage = "Amount must be greater than zero";
                    await _logger.LogAsync(LogLevel.Error, errorMessage);
                    throw new ArgumentException(errorMessage);
                }

                await _logger.LogAsync(LogLevel.Information, $"Claim validation passed: Type={command.ClaimType}, Amount={command.Amount}");

                int? userId = command.UserId;
                if (userId.HasValue)
                {
                    try
                    {
                        await _logger.LogAsync(LogLevel.Information, $"Looking up user with ID: {userId.Value}");
                        User user = await _userRepository.GetByIdAsync(userId.Value);

                        if (user == null)
                        {
                            await _logger.LogAsync(LogLevel.Warning, $"User with ID {userId.Value} not found, creating new user");
                            var autoNumber = await _userRepository.GetNextAutoNumberAsync();
                            var newUser = new User
                            {
                                FullName = $"Test {autoNumber}",
                                Email = $"test{autoNumber}@example.com",
                                Phone = "N/A",
                                DateJoined = DateTime.UtcNow
                            };

                            var createdUser = await _userRepository.AddAsync(newUser);
                            userId = createdUser.Id;
                            await _logger.LogAsync(LogLevel.Information, $"Created new user with ID: {userId}");
                        }
                        else
                        {
                            userId = user.Id;
                            await _logger.LogAsync(LogLevel.Information, $"Found existing user: {user.FullName} (ID: {userId})");
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogAsync(LogLevel.Error, $"Error during user lookup/creation: {ex.Message}", ex);
                        throw;
                    }
                }

                var claim = new Claim
                {
                    ClaimType = command.ClaimType,
                    Amount = Money.Create(command.Amount),
                    DateFiled = command.DateFiled,
                    Status = "Pending",
                    UserId = userId
                };

                await _logger.LogAsync(LogLevel.Information, $"Creating claim entity: Type={claim.ClaimType}, Amount={claim.Amount.Amount}, UserId={claim.UserId}");

                var submittedClaim = await _claimRepository.AddAsync(claim);
                await _logger.LogAsync(LogLevel.Information, $"Claim successfully submitted with ID: {submittedClaim.Id}");

                var result = new ClaimViewModel
                {
                    Id = submittedClaim.Id,
                    ClaimType = submittedClaim.ClaimType,
                    Amount = submittedClaim.Amount.Amount,
                    DateFiled = submittedClaim.DateFiled,
                    Status = submittedClaim.Status,
                    UserId = submittedClaim.UserId,
                    UserName = submittedClaim.User?.FullName ?? "Anonymous",
                    UserEmail = submittedClaim.User?.Email ?? "N/A",
                    CreatedAt = submittedClaim.CreatedAt
                };

                await _logger.LogAsync(LogLevel.Information, $"Claim submission completed successfully. Claim ID: {result.Id}");
                return result;
            }
            catch (ArgumentException ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Validation error in SubmitClaimAsync: {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Unexpected error in SubmitClaimAsync: {ex.Message}", ex);
                throw new InvalidOperationException("An error occurred while submitting the claim", ex);
            }
        }

        public async Task<ClaimViewModel> GetClaimByIdAsync(int id)
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, $"Retrieving claim by ID: {id}");
                
                var claim = await _claimRepository.GetByIdAsync(id);

                if (claim == null)
                {
                    await _logger.LogAsync(LogLevel.Warning, $"Claim with ID {id} not found");
                    return null;
                }

                await _logger.LogAsync(LogLevel.Information, $"Successfully retrieved claim: ID={claim.Id}, Type={claim.ClaimType}, Status={claim.Status}");

                return new ClaimViewModel
                {
                    Id = claim.Id,
                    ClaimType = claim.ClaimType,
                    Amount = claim.Amount.Amount,
                    DateFiled = claim.DateFiled,
                    Status = claim.Status,
                    UserId = claim.UserId,
                    UserName = claim.User?.FullName ?? "Anonymous",
                    UserEmail = claim.User?.Email ?? "N/A",
                    CreatedAt = claim.CreatedAt
                };
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error retrieving claim by ID {id}: {ex.Message}", ex);
                throw new InvalidOperationException($"An error occurred while retrieving claim with ID {id}", ex);
            }
        }

        public async Task<IEnumerable<ClaimViewModel>> GetAllClaimsAsync()
        {
            try
            {
                await _logger.LogAsync(LogLevel.Information, "Retrieving all claims");
                
                var claims = await _claimRepository.GetAllAsync();
                await _logger.LogAsync(LogLevel.Information, $"Successfully retrieved {claims.Count()} claims");

                var result = claims.Select(claim => new ClaimViewModel
                {
                    Id = claim.Id,
                    ClaimType = claim.ClaimType,
                    Amount = claim.Amount.Amount,
                    DateFiled = claim.DateFiled,
                    Status = claim.Status,
                    UserId = claim.UserId,
                    UserName = claim.User?.FullName ?? "Anonymous",
                    UserEmail = claim.User?.Email ?? "N/A",
                    CreatedAt = claim.CreatedAt
                }).ToList();

                await _logger.LogAsync(LogLevel.Information, $"Returning {result.Count} claim view models");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Error retrieving all claims: {ex.Message}", ex);
                throw new InvalidOperationException("An error occurred while retrieving all claims", ex);
            }
        }
    }
}
