using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Models;
using InsuranceFraudDetection.Application.Claims.Commands;
using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Core.Entities;
using InsuranceFraudDetection.Core.ValueObjects;

namespace InsuranceFraudDetection.Application.Claims.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IUserRepository _userRepository;

        public ClaimService(IClaimRepository claimRepository, IUserRepository userRepository)
        {
            _claimRepository = claimRepository;
            _userRepository = userRepository;
        }

        public async Task<ClaimViewModel> SubmitClaimAsync(SubmitClaimCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.ClaimType))
                throw new ArgumentException("Claim type is required");

            if (command.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            int? userId = command.UserId;
            if (userId.HasValue)
            {
                User user = await _userRepository.GetByIdAsync(userId.Value);

                if (user == null)
                {
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
                }else
                {
                    userId = user.Id;
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

            var submittedClaim = await _claimRepository.AddAsync(claim);

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
            var claim = await _claimRepository.GetByIdAsync(id);

            if (claim == null)
                return null;

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
