using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InsuranceFraudDetection.Infrastructure.Logging
{
    public static class LoggingServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomLogging(this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddProvider(new CustomLoggerProvider());
            });

            return services;
        }

        public static IServiceCollection AddCustomLogging(this IServiceCollection services, Action<ILoggingBuilder> configure)
        {
            services.AddLogging(builder =>
            {
                builder.AddProvider(new CustomLoggerProvider());
                configure(builder);
            });

            return services;
        }
    }
}
