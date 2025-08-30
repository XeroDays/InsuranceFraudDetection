using InsuranceFraudDetection.Application.Claims.Models;
using InsuranceFraudDetection.Application.Claims.Commands;

namespace InsuranceFraudDetection.Application.Claims.Interfaces
{
    public interface IClaimService
    {
        Task<ClaimViewModel> SubmitClaimAsync(SubmitClaimCommand command);
        Task<ClaimViewModel> GetClaimByIdAsync(int id);
        Task<IEnumerable<ClaimViewModel>> GetAllClaimsAsync();
    }
}
