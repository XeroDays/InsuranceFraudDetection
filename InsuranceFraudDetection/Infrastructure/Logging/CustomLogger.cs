using Microsoft.Extensions.Logging;
using System.Text;

namespace InsuranceFraudDetection.Infrastructure.Logging
{
    public class CustomLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        public CustomLogger(string categoryName)
        {
            _categoryName = categoryName;
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");
            
            // Ensure the Logs directory exists
            var logsDirectory = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory!);
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;  
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var logEntry = FormatLogEntry(logLevel, message, exception);
            
            WriteToFile(logEntry);
        }

        private string FormatLogEntry(LogLevel logLevel, string message, Exception? exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logLevelString = logLevel.ToString().ToUpper();
            
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{timestamp}] [{logLevelString}] [{_categoryName}] {message}");
            
            if (exception != null)
            {
                logEntry.AppendLine($"Exception: {exception.Message}");
                logEntry.AppendLine($"StackTrace: {exception.StackTrace}");
                
                var innerException = exception.InnerException;
                while (innerException != null)
                {
                    logEntry.AppendLine($"Inner Exception: {innerException.Message}");
                    innerException = innerException.InnerException;
                }
            }
            
            return logEntry.ToString();
        }

        private void WriteToFile(string logEntry)
        {
            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, logEntry, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    // Fallback to console if file writing fails
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                    Console.WriteLine($"Log entry: {logEntry}");
                }
            }
        }
    }
}
