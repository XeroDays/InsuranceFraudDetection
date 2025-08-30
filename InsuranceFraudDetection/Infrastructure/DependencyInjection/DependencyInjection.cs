using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Services;
using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Infrastructure.Data;
using InsuranceFraudDetection.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceFraudDetection.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<InsuranceDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IClaimRepository, ClaimRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClaimService, ClaimService>();

            return services;
        }
    }
}
