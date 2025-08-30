using System.ComponentModel.DataAnnotations;

namespace InsuranceFraudDetection.WebAPI.Models
{
    public class ClaimApiModel
    {
        public int Id { get; set; }
        public string ClaimType { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateFiled { get; set; }
        public string Status { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SubmitClaimApiModel
    {
        [Required]
        public string ClaimType { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public int? UserId { get; set; }
    }
}
