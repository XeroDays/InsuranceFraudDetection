using FluentAssertions;
using InsuranceFraudDetection.Application.Claims.Commands;
using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Models;
using InsuranceFraudDetection.Application.Claims.Services;
using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Core.Entities;
using InsuranceFraudDetection.Core.ValueObjects;
using Moq;
using Xunit;

namespace InsuranceFraudDetection.Tests.Application.Claims.Services
{
    public class ClaimServiceTests
    {
        private readonly Mock<IClaimRepository> _mockClaimRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly ClaimService _claimService;

        public ClaimServiceTests()
        {
            _mockClaimRepository = new Mock<IClaimRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _claimService = new ClaimService(_mockClaimRepository.Object, _mockUserRepository.Object);
        }

        [Fact]
        public async Task SubmitClaimAsync_WithValidCommand_ShouldInsertClaim()
        {  
            var command = new SubmitClaimCommand
            {
                ClaimType = "Auto",
                Amount = 5000.00m,
                UserId = 1,
                DateFiled = DateTime.UtcNow
            };

            var user = new User
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "123-456-7890",
                DateJoined = DateTime.UtcNow.AddDays(-30)
            };

            var expectedClaim = new Claim
            {
                Id = 1,
                ClaimType = command.ClaimType,
                Amount = Money.Create(command.Amount),
                DateFiled = command.DateFiled,
                Status = "Pending",
                UserId = user.Id,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _mockClaimRepository.Setup(x => x.AddAsync(It.IsAny<Claim>()))
                .ReturnsAsync(expectedClaim);
             
            var result = await _claimService.SubmitClaimAsync(command);
             
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.ClaimType.Should().Be("Auto");
            result.Amount.Should().Be(5000.00m);
            result.Status.Should().Be("Pending");
            result.UserId.Should().Be(1);
            result.UserName.Should().Be("John Doe");
            result.UserEmail.Should().Be("john@example.com");
             
            _mockUserRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
            _mockClaimRepository.Verify(x => x.AddAsync(It.IsAny<Claim>()), Times.Once);
        }

        [Fact]
        public async Task SubmitClaimAsync_WithZeroAmount_ShouldNotBeAcceptable()
        { 
            var command = new SubmitClaimCommand
            {
                ClaimType = "Auto",
                Amount = 0,
                UserId = 1,
                DateFiled = DateTime.UtcNow
            };
             
            var exception = await Assert.ThrowsAsync<ArgumentException>(  () => _claimService.SubmitClaimAsync(command));
             
            exception.Message.Should().Be("Amount must be greater than zero");
             
            _mockClaimRepository.Verify(x => x.AddAsync(It.IsAny<Claim>()), Times.Never);
        }
    }
}
