namespace InsuranceFraudDetection.Core.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateJoined { get; set; }

        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
