using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Models;
using InsuranceFraudDetection.Application.Claims.Commands;
using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Core.Entities;
using InsuranceFraudDetection.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace InsuranceFraudDetection.Application.Claims.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ClaimService> _logger;

        public ClaimService(IClaimRepository claimRepository, IUserRepository userRepository, ILogger<ClaimService> logger)
        {
            _claimRepository = claimRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ClaimViewModel> SubmitClaimAsync(SubmitClaimCommand command)
        {
            _logger.LogInformation("Starting claim submission for type: {ClaimType}, amount: {Amount}", command.ClaimType, command.Amount);
            
            if (string.IsNullOrWhiteSpace(command.ClaimType))
            {
                _logger.LogWarning("Claim submission failed: Claim type is required");
                throw new ArgumentException("Claim type is required");
            }

            if (command.Amount <= 0)
            {
                _logger.LogWarning("Claim submission failed: Amount must be greater than zero");
                throw new ArgumentException("Amount must be greater than zero");
            }

            int? userId = command.UserId;
            if (userId.HasValue)
            {
                _logger.LogInformation("Looking up user with ID: {UserId}", userId.Value);
                User user = await _userRepository.GetByIdAsync(userId.Value);

                if (user == null)
                {
                    _logger.LogInformation("User not found, creating new user");
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
                    _logger.LogInformation("New user created with ID: {UserId}", userId);
                }else
                {
                    userId = user.Id;
                    _logger.LogInformation("Existing user found with ID: {UserId}", userId);
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

            _logger.LogInformation("Creating claim with type: {ClaimType}, amount: {Amount}, userId: {UserId}", 
                claim.ClaimType, claim.Amount.Amount, userId);

            var submittedClaim = await _claimRepository.AddAsync(claim);
            
            _logger.LogInformation("Claim successfully submitted with ID: {ClaimId}", submittedClaim.Id);

            return new ClaimViewModel
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
        }

        public async Task<ClaimViewModel> GetClaimByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving claim with ID: {ClaimId}", id);
            
            var claim = await _claimRepository.GetByIdAsync(id);

            if (claim == null)
            {
                _logger.LogWarning("Claim not found with ID: {ClaimId}", id);
                return null;
            }

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

        public async Task<IEnumerable<ClaimViewModel>> GetAllClaimsAsync()
        {
            _logger.LogInformation("Retrieving all claims");
            
            var claims = await _claimRepository.GetAllAsync();

            return claims.Select(claim => new ClaimViewModel
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
            });
        }
    }
}
