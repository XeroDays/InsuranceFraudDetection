# Custom Logger Implementation

This folder contains a custom logging implementation that writes logs to the `Logs/log.txt` file.

## Files

- **CustomLogger.cs**: Implements the `ILogger` interface and handles writing logs to the file system
- **CustomLoggerProvider.cs**: Implements `ILoggerProvider` to create logger instances
- **LoggingServiceCollectionExtensions.cs**: Extension methods for easy registration with DI container

## Features

- **File-based logging**: All logs are written to `Logs/log.txt`
- **Thread-safe**: Uses locks to ensure thread safety when writing to files
- **Structured logging**: Supports structured logging with parameters
- **Exception handling**: Detailed exception logging with stack traces and inner exceptions
- **Automatic directory creation**: Creates the Logs directory if it doesn't exist
- **Fallback mechanism**: Falls back to console output if file writing fails

## Usage

### 1. Registration

The custom logger is automatically registered when you call `AddInfrastructure()` in your `Program.cs`:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

### 2. Using in Services

Inject `ILogger<T>` into your services:

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        _logger.LogInformation("Starting operation");
        
        try
        {
            // Your code here
            _logger.LogInformation("Operation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed");
            throw;
        }
    }
}
```

### 3. Log Levels

The custom logger supports all standard log levels:
- `LogTrace`
- `LogDebug`
- `LogInformation`
- `LogWarning`
- `LogError`
- `LogCritical`

### 4. Structured Logging

Use structured logging with parameters:

```csharp
_logger.LogInformation("Processing claim {ClaimId} for user {UserId}", claimId, userId);
```

### 5. Exception Logging

Log exceptions with full details:

```csharp
try
{
    // Your code
}
catch (Exception ex)
{
    _logger.LogError(ex, "An error occurred while processing the request");
}
```

## Log Format

Logs are written in the following format:

```
[2024-01-15 14:30:45.123] [INFORMATION] [ClaimService] Starting claim submission for type: Auto, amount: 5000
[2024-01-15 14:30:45.124] [ERROR] [ClaimService] An error occurred while processing the request
Exception: Database connection failed
StackTrace: at ClaimService.SubmitClaimAsync(SubmitClaimCommand command) in ClaimService.cs:line 45
```

## Configuration

The logger automatically:
- Creates the `Logs` directory if it doesn't exist
- Uses UTF-8 encoding for log files
- Appends to the existing log file
- Handles file write errors gracefully

## Example Output

When you run the application and perform operations, you'll see logs like:

```
[2024-01-15 14:30:45.123] [INFORMATION] [ClaimService] Starting claim submission for type: Auto, amount: 5000
[2024-01-15 14:30:45.124] [INFORMATION] [ClaimService] Looking up user with ID: 1
[2024-01-15 14:30:45.125] [INFORMATION] [ClaimService] Existing user found with ID: 1
[2024-01-15 14:30:45.126] [INFORMATION] [ClaimService] Creating claim with type: Auto, amount: 5000, userId: 1
[2024-01-15 14:30:45.127] [INFORMATION] [ClaimService] Claim successfully submitted with ID: 123
```
