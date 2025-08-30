using InsuranceFraudDetection.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceFraudDetection.Infrastructure.Data
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClaimType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Amount).HasConversion(
                    v => v.Amount,
                    v => new Core.ValueObjects.Money(v, "USD")
                );
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).IsRequired(false);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Claims)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
            });
        }
    }
}
