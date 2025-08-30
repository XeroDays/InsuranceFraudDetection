using System.ComponentModel.DataAnnotations;

namespace InsuranceFraudDetection.Core.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    }
}
