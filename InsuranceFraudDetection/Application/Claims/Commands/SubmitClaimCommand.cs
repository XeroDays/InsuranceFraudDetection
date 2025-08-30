namespace InsuranceFraudDetection.Application.Claims.Commands
{
    public class SubmitClaimCommand
    {
        public string ClaimType { get; set; }
        public decimal Amount { get; set; }
        public int? UserId { get; set; }
        public DateTime DateFiled { get; set; } = DateTime.UtcNow;
    }
}
