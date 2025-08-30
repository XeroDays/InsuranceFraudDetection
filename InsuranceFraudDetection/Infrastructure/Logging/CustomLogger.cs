using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace InsuranceFraudDetection.Infrastructure.Logging
{
    public class CustomLoggerOptions
    {
        public bool Enabled { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/application.log";
        public int BatchSize { get; set; } = 500;
        public int FlushIntervalSeconds { get; set; } = 5;
        public int MaxQueueSize { get; set; } = 10000;
        public long MaxFileSizeBytes { get; set; } = 104857600; // 100MB
        public int MaxArchiveFiles { get; set; } = 10;
        public bool IncludeScopes { get; set; } = true;
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
    }

    public readonly struct LogEntry
    {
        public DateTime Timestamp { get; }
        public LogLevel Level { get; }
        public string Category { get; }
        public string Message { get; }
        public string SessionId { get; }
        public string TraceId { get; }
        public Exception Exception { get; }
        public string CallerMemberName { get; }
        public string CallerFilePath { get; }
        public int CallerLineNumber { get; }

        public LogEntry(LogLevel level, string category, string message, string sessionId, string traceId = null, Exception exception = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Timestamp = DateTime.UtcNow;
            Level = level;
            Category = category;
            Message = message;
            SessionId = sessionId;
            TraceId = traceId ?? Activity.Current?.Id;
            Exception = exception;
            CallerMemberName = callerMemberName;
            CallerFilePath = Path.GetFileName(callerFilePath);
            CallerLineNumber = callerLineNumber;
        }
    }


    public class LogStatistics
    {
        public long TotalLogsWritten { get; set; }
        public long TotalLogsFailed { get; set; }
        public int QueueLength { get; set; }
        public long CurrentFileSizeBytes { get; set; }
        public TimeSpan AverageWriteTime { get; set; }
        public DateTime LastWriteTime { get; set; }
    }


    public interface ICustomLogger : IDisposable
    {
        string SessionId { get; }
        Task LogAsync(LogLevel level, string message, Exception exception = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0);
        Task LogAsync(LogEntry entry);
        Task FlushAsync(CancellationToken cancellationToken = default);
        Task<LogStatistics> GetStatisticsAsync();
        Task<string> GetLogs();
    }

    public sealed class CustomLogger : ICustomLogger, IHostedService
    {
        private readonly CustomLoggerOptions _options;
        private readonly Channel<LogEntry> _logChannel;
        private readonly SemaphoreSlim _writeSemaphore;
        private readonly Timer _flushTimer;
        private readonly string _sessionId;
        private readonly ILogger<CustomLogger> _fallbackLogger;

        private CancellationTokenSource _shutdownTokenSource;
        private Task _processingTask;
        private StreamWriter _currentWriter;
        private FileStream _currentFileStream;
        private long _currentFileSize;
        private readonly object _writerLock = new();

        // Statistics
        private long _totalLogsWritten;
        private long _totalLogsFailed;
        private DateTime _lastWriteTime;
        private readonly ConcurrentQueue<TimeSpan> _recentWriteTimes;
        private const int MaxRecentWriteSamples = 100;

        // Flush coordination
        private volatile bool _forceFlush = false;
        private readonly object _flushLock = new();

        public string SessionId => _sessionId;

        public CustomLogger(IOptions<CustomLoggerOptions> options, ILogger<CustomLogger> fallbackLogger = null)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _fallbackLogger = fallbackLogger;
            _sessionId = Guid.NewGuid().ToString("N");
            _writeSemaphore = new SemaphoreSlim(1, 1);
            _recentWriteTimes = new ConcurrentQueue<TimeSpan>();

            // Create bounded channel for back-pressure support
            var channelOptions = new BoundedChannelOptions(_options.MaxQueueSize)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };
            _logChannel = Channel.CreateBounded<LogEntry>(channelOptions);

            // Initialize flush timer
            _flushTimer = new Timer(
                _ => Task.Run(() => TriggerFlush()),
                null,
                TimeSpan.FromSeconds(_options.FlushIntervalSeconds),
                TimeSpan.FromSeconds(_options.FlushIntervalSeconds));
        }

        private async Task InitializeLogFileAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_options.LogFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Check if existing log file exists and is not empty, then rotate it
                await RotateExistingLogFileIfNeededAsync();

                OpenLogFile();
            }
            catch (Exception ex)
            {
                _fallbackLogger?.LogError(ex, "Failed to initialize log file");
            }
        }

        private async Task RotateExistingLogFileIfNeededAsync()
        {
            try
            {
                if (File.Exists(_options.LogFilePath))
                {
                    var fileInfo = new FileInfo(_options.LogFilePath);

                    // Check if the file is not empty (has content)
                    if (fileInfo.Length > 0)
                    {
                        // Generate timestamp for the archive filename
                        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                        var archivePath = Path.Combine(
                            Path.GetDirectoryName(_options.LogFilePath),
                            $"{Path.GetFileNameWithoutExtension(_options.LogFilePath)}_{timestamp}{Path.GetExtension(_options.LogFilePath)}");

                        // Rename the existing file with timestamp
                        File.Move(_options.LogFilePath, archivePath);

                        // Clean up old archives to maintain the configured limit
                        CleanupOldArchives();

                        // Log the rotation (this will be written to the new file)
                        var rotationMessage = $"Log file rotated on application startup. Previous log archived as: {Path.GetFileName(archivePath)}";
                        _fallbackLogger?.LogInformation(rotationMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _fallbackLogger?.LogError(ex, "Failed to rotate existing log file on startup");
            }
        }

        private void OpenLogFile()
        {
            lock (_writerLock)
            {
                CloseCurrentWriter();

                _currentFileStream = new FileStream(_options.LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize: 65536, useAsync: true);

                _currentWriter = new StreamWriter(_currentFileStream, Encoding.UTF8, bufferSize: 65536, leaveOpen: false);
                _currentFileSize = _currentFileStream.Length;
            }
        }

        private void CloseCurrentWriter()
        {
            _currentWriter?.Dispose();
            _currentFileStream?.Dispose();
            _currentWriter = null;
            _currentFileStream = null;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Ensure log file is initialized before starting
            await InitializeLogFileAsync();

            _shutdownTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _processingTask = ProcessLogsAsync(_shutdownTokenSource.Token);

            await LogAsync(LogLevel.Information, $"CustomLogger started with session: {_sessionId}");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _flushTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _logChannel.Writer.TryComplete();

            _shutdownTokenSource?.Cancel();

            try
            {
                await (_processingTask ?? Task.CompletedTask).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }

            await FlushAsync(cancellationToken);
            CloseCurrentWriter();

            _flushTimer?.Dispose();
            _writeSemaphore?.Dispose();
            _shutdownTokenSource?.Dispose();
        }

        public async Task LogAsync(LogLevel level, string message, Exception exception = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (!_options.Enabled || level < _options.MinimumLevel)
                return;

            var entry = new LogEntry(level, GetType().Name, message, _sessionId, exception: exception, callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber);

            await LogAsync(entry);
        }

        public async Task LogAsync(LogEntry entry)
        {
            if (!_options.Enabled || entry.Level < _options.MinimumLevel)
                return;

            try
            {
                bool status = _logChannel.Writer.TryWrite(entry);

                if (!status)
                {
                    // Channel is full, try async write with timeout
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await _logChannel.Writer.WriteAsync(entry, cts.Token);
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalLogsFailed);
                _fallbackLogger?.LogError(ex, "Failed to enqueue log entry");
            }
        }

        private async Task ProcessLogsAsync(CancellationToken cancellationToken)
        {
            var batch = new List<LogEntry>(_options.BatchSize);

            try
            {
                await foreach (var entry in _logChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    batch.Add(entry);

                    // Check if we should flush (either batch size reached or force flush triggered)
                    if (batch.Count >= _options.BatchSize || _forceFlush)
                    {
                        await WriteBatchAsync(batch, cancellationToken);
                        batch.Clear();

                        // Reset force flush flag
                        if (_forceFlush)
                        {
                            lock (_flushLock)
                            {
                                _forceFlush = false;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
            catch (Exception ex)
            {
                _fallbackLogger?.LogError(ex, "Error in log processing loop");
            }
            finally
            {
                // Write any remaining logs
                if (batch.Count > 0)
                {
                    await WriteBatchAsync(batch, cancellationToken);
                }
            }
        }

        private void TriggerFlush()
        {
            lock (_flushLock)
            {
                _forceFlush = true;
            }
        }

        private async Task WriteBatchAsync(List<LogEntry> batch, CancellationToken cancellationToken)
        {
            if (batch.Count == 0)
                return;

            var stopwatch = Stopwatch.StartNew();

            await _writeSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Check for file rotation
                await CheckAndRotateLogFileAsync();

                var sb = new StringBuilder(batch.Count * 200);

                foreach (var entry in batch)
                {
                    FormatLogEntry(sb, entry);
                }

                await WriteToFileAsync(sb.ToString(), cancellationToken);

                Interlocked.Add(ref _totalLogsWritten, batch.Count);
                _lastWriteTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Interlocked.Add(ref _totalLogsFailed, batch.Count);
                _fallbackLogger?.LogError(ex, "Failed to write log batch");
            }
            finally
            {
                _writeSemaphore.Release();

                stopwatch.Stop();
                RecordWriteTime(stopwatch.Elapsed);
            }
        }

        private void FormatLogEntry(StringBuilder sb, LogEntry entry)
        {
            string timestamp = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}]";
            string level = $"[{entry.Level}]";
            string sessionId = $"[{entry.SessionId.Substring(0, 8)}]";
            string traceId = !string.IsNullOrEmpty(entry.TraceId) ? $" [TraceId: {entry.TraceId}]" : "";
            string category = $"[{entry.Category}]";
            string scope = _options.IncludeScopes && !string.IsNullOrEmpty(entry.CallerMemberName) ? $" [{entry.CallerFilePath}:{entry.CallerMemberName}:{entry.CallerLineNumber}]" : "";

            string logLine = $"{timestamp} {level} {traceId} {scope} {entry.Message}";
            sb.AppendLine(logLine);

            if (entry.Exception != null)
            {
                sb.AppendLine("Exception Details:");
                sb.AppendLine(entry.Exception.ToString());
            }
        }

        private async Task WriteToFileAsync(string content, CancellationToken cancellationToken)
        {
            lock (_writerLock)
            {
                if (_currentWriter == null)
                {
                    OpenLogFile();
                }
            }

            await _currentWriter.WriteAsync(content.AsMemory(), cancellationToken);
            await _currentWriter.FlushAsync();

            _currentFileSize += Encoding.UTF8.GetByteCount(content);
        }

        private async Task CheckAndRotateLogFileAsync()
        {
            if (_currentFileSize < _options.MaxFileSizeBytes)
                return;

            lock (_writerLock)
            {
                CloseCurrentWriter();

                // Rotate log files
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var archivePath = Path.Combine(
                    Path.GetDirectoryName(_options.LogFilePath),
                    $"{Path.GetFileNameWithoutExtension(_options.LogFilePath)}_{timestamp}{Path.GetExtension(_options.LogFilePath)}");

                try
                {
                    File.Move(_options.LogFilePath, archivePath);
                    CleanupOldArchives();
                }
                catch (Exception ex)
                {
                    _fallbackLogger?.LogError(ex, "Failed to rotate log file");
                }

                OpenLogFile();
            }
        }

        private void CleanupOldArchives()
        {
            try
            {
                var directory = Path.GetDirectoryName(_options.LogFilePath);
                var pattern = $"{Path.GetFileNameWithoutExtension(_options.LogFilePath)}_*{Path.GetExtension(_options.LogFilePath)}";
                var archives = Directory.GetFiles(directory, pattern).OrderByDescending(f => File.GetCreationTimeUtc(f)).Skip(_options.MaxArchiveFiles);
                foreach (var archive in archives)
                {
                    File.Delete(archive);
                }
            }
            catch (Exception ex)
            {
                _fallbackLogger?.LogError(ex, "Failed to cleanup old archives");
            }
        }

        private void RecordWriteTime(TimeSpan writeTime)
        {
            _recentWriteTimes.Enqueue(writeTime);

            while (_recentWriteTimes.Count > MaxRecentWriteSamples && _recentWriteTimes.TryDequeue(out _))
            {
                // Remove old samples
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            var entries = new List<LogEntry>();

            while (_logChannel.Reader.TryRead(out var entry))
            {
                entries.Add(entry);
            }

            if (entries.Count > 0)
            {
                await WriteBatchAsync(entries, cancellationToken);
            }
        }

        public Task<LogStatistics> GetStatisticsAsync()
        {
            var stats = new LogStatistics
            {
                TotalLogsWritten = _totalLogsWritten,
                TotalLogsFailed = _totalLogsFailed,
                QueueLength = _logChannel.Reader.Count,
                CurrentFileSizeBytes = _currentFileSize,
                LastWriteTime = _lastWriteTime
            };

            if (_recentWriteTimes.Count > 0)
            {
                stats.AverageWriteTime = TimeSpan.FromMilliseconds(
                    _recentWriteTimes.Average(t => t.TotalMilliseconds));
            }

            return Task.FromResult(stats);
        }


        public void Dispose()
        {
            StopAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public async Task<string> GetLogs()
        {
            try
            { 
                await FlushAsync();
                 
                using var readStream = new FileStream(
                    _options.LogFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    bufferSize: 65536,
                    useAsync: true);

                using var reader = new StreamReader(readStream, Encoding.UTF8); 
                var logs = await reader.ReadToEndAsync();

                return logs;
            }
            catch (FileNotFoundException)
            {
                return "Log file not found.";
            }
            catch (IOException ex)
            {
                _fallbackLogger?.LogError(ex, "Failed to read logs due to I/O error");
                return $"Error reading logs: {ex.Message}";
            }
            catch (Exception ex)
            {
                _fallbackLogger?.LogError(ex, "Unexpected error while reading logs");
                return $"Error reading logs: {ex.Message}";
            }
        }
    }

    public static class CustomLoggerExtensions
    {
        public static IServiceCollection AddCustomLogger(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CustomLoggerOptions>(
                configuration.GetSection("CustomLogger"));

            services.AddSingleton<CustomLogger>();
            services.AddSingleton<ICustomLogger>(provider => provider.GetService<CustomLogger>());
            services.AddHostedService(provider => provider.GetService<CustomLogger>());

            return services;
        }

        public static IServiceCollection AddCustomLogger(this IServiceCollection services, Action<CustomLoggerOptions> configureOptions)
        {
            services.Configure(configureOptions);

            services.AddSingleton<CustomLogger>();
            services.AddSingleton<ICustomLogger>(provider => provider.GetService<CustomLogger>());
            services.AddHostedService(provider => provider.GetService<CustomLogger>());

            return services;
        }
    }
}
