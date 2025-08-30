using Microsoft.AspNetCore.Mvc;
using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Models;
using InsuranceFraudDetection.Application.Claims.Commands;
using InsuranceFraudDetection.WebAPI.Models;

namespace InsuranceFraudDetection.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimsApiController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsApiController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClaimApiModel>>> GetAllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            var apiModels = claims.Select(c => new ClaimApiModel
            {
                Id = c.Id,
                ClaimType = c.ClaimType,
                Amount = c.Amount,
                DateFiled = c.DateFiled,
                Status = c.Status,
                UserId = c.UserId,
                UserName = c.UserName,
                CreatedAt = c.CreatedAt
            });

            return Ok(apiModels);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClaimApiModel>> GetClaimById(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
                return NotFound();

            var apiModel = new ClaimApiModel
            {
                Id = claim.Id,
                ClaimType = claim.ClaimType,
                Amount = claim.Amount,
                DateFiled = claim.DateFiled,
                Status = claim.Status,
                UserId = claim.UserId,
                UserName = claim.UserName,
                CreatedAt = claim.CreatedAt
            };

            return Ok(apiModel);
        }

        [HttpPost]
        public async Task<ActionResult<ClaimApiModel>> SubmitClaim(SubmitClaimApiModel model)
        {
            var command = new SubmitClaimCommand
            {
                ClaimType = model.ClaimType,
                Amount = model.Amount,
                UserId = model.UserId
            };

            var result = await _claimService.SubmitClaimAsync(command);
            
            var apiModel = new ClaimApiModel
            {
                Id = result.Id,
                ClaimType = result.ClaimType,
                Amount = result.Amount,
                DateFiled = result.DateFiled,
                Status = result.Status,
                UserId = result.UserId,
                UserName = result.UserName,
                CreatedAt = result.CreatedAt
            };

            return CreatedAtAction(nameof(GetClaimById), new { id = result.Id }, apiModel);
        }
    }
}
