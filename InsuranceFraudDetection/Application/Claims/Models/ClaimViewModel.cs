namespace InsuranceFraudDetection.Application.Claims.Models
{
    public class ClaimViewModel
    {
        public int Id { get; set; }
        public string ClaimType { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateFiled { get; set; }
        public string Status { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
