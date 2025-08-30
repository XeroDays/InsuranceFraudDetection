using InsuranceFraudDetection.Core.ValueObjects;

namespace InsuranceFraudDetection.Core.Entities
{
    public class Claim : BaseEntity
    {
        public string ClaimType { get; set; }
        public Money Amount { get; set; }
        public DateTime DateFiled { get; set; }
        public string Status { get; set; }
        public int? UserId { get; set; }
        
        public User? User { get; set; }
    }
}
