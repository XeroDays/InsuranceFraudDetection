using System.ComponentModel.DataAnnotations;

namespace InsuranceFraudDetection.Presentation.Models
{
    public class SubmitClaimViewModel
    {
        [Required(ErrorMessage = "Claim type is required")]
        [Display(Name = "Claim Type")]
        public string ClaimType { get; set; }
        
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        
        [Display(Name = "User ID (Optional for anonymous claims)")]
        public int? UserId { get; set; }
    }
}
