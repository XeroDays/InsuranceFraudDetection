using Microsoft.Extensions.Logging;

namespace InsuranceFraudDetection.Infrastructure.Logging
{
    public class CustomLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName);
        }

        public void Dispose()
        {
            // Cleanup resources if needed
        }
    }
}
