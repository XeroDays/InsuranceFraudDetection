using InsuranceFraudDetection.Application.Claims.Interfaces;
using InsuranceFraudDetection.Application.Claims.Services;
using InsuranceFraudDetection.Application.Interfaces;
using InsuranceFraudDetection.Infrastructure.Data;
using InsuranceFraudDetection.Infrastructure.Logging;
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

            // Register repositories with ICustomLogger dependency
            services.AddScoped<IClaimRepository>(provider => 
                new ClaimRepository(
                    provider.GetRequiredService<InsuranceDbContext>(),
                    provider.GetRequiredService<ICustomLogger>()
                ));
            
            services.AddScoped<IUserRepository>(provider => 
                new UserRepository(
                    provider.GetRequiredService<InsuranceDbContext>(),
                    provider.GetRequiredService<ICustomLogger>()
                ));

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register ClaimService with ICustomLogger dependency
            services.AddScoped<IClaimService>(provider => 
                new ClaimService(
                    provider.GetRequiredService<IClaimRepository>(),
                    provider.GetRequiredService<IUserRepository>(),
                    provider.GetRequiredService<ICustomLogger>()
                ));

            return services;
        }
    }
}
