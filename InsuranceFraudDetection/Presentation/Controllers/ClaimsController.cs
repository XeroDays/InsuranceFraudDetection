using Microsoft.AspNetCore.Mvc;
using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Models;
using InsuranceFraudDetection.Presentation.Models;

namespace InsuranceFraudDetection.Presentation.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        public async Task<IActionResult> Index()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return View(claims);
        }

        public IActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(SubmitClaimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var command = new Application.Claims.Commands.SubmitClaimCommand
            {
                ClaimType = model.ClaimType,
                Amount = model.Amount,
                UserId = model.UserId
            };

            var result = await _claimService.SubmitClaimAsync(command);
            return RedirectToAction(nameof(Success), new { id = result.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        public IActionResult Success(int id)
        {
            ViewBag.ClaimId = id;
            return View();
        }
    }
}
