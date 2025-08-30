# Health Controller API Endpoints

The HealthController provides health monitoring and administrative endpoints for the Insurance Fraud Detection application.

## Endpoints

### 1. Get Application Logs
**GET** `/api/health/logs`

Retrieves all application logs from the `Logs/log.txt` file.

#### Response Format
```json
{
  "success": true,
  "message": "Logs retrieved successfully",
  "logs": [
    "[2024-01-15 14:30:45.123] [INFORMATION] [ClaimService] Starting claim submission for type: Auto, amount: 5000",
    "[2024-01-15 14:30:45.124] [INFORMATION] [ClaimService] Looking up user with ID: 1",
    "[2024-01-15 14:30:45.125] [INFORMATION] [ClaimService] Existing user found with ID: 1"
  ],
  "totalLines": 3,
  "retrievedAt": "2024-01-15T14:30:45.000Z"
}
```

#### Response Properties
- `success`: Boolean indicating if the operation was successful
- `message`: Human-readable message describing the result
- `logs`: Array of log entries as strings
- `totalLines`: Total number of log entries retrieved
- `retrievedAt`: Timestamp when the logs were retrieved

#### HTTP Status Codes
- `200 OK`: Logs retrieved successfully
- `404 Not Found`: Log file doesn't exist
- `500 Internal Server Error`: Error occurred while reading logs

#### Example Usage
```bash
curl -X GET "http://localhost:5000/api/health/logs"
```

### 2. Shutdown Server
**POST** `/api/health/shutdown`

Initiates a graceful shutdown of the application server.

#### Response
```json
"Server shutdown initiated"
```

#### Example Usage
```bash
curl -X POST "http://localhost:5000/api/health/shutdown"
```

## Log Format

The logs returned by the `/api/health/logs` endpoint follow this format:

```
[Timestamp] [LogLevel] [Category] Message
```

Example:
```
[2024-01-15 14:30:45.123] [INFORMATION] [ClaimService] Starting claim submission for type: Auto, amount: 5000
[2024-01-15 14:30:45.124] [WARNING] [ClaimService] Claim submission failed: Claim type is required
[2024-01-15 14:30:45.125] [ERROR] [ClaimService] An error occurred while processing the request
Exception: Database connection failed
StackTrace: at ClaimService.SubmitClaimAsync(SubmitClaimCommand command) in ClaimService.cs:line 45
```

## Security Considerations

⚠️ **Important**: The logs endpoint may contain sensitive information. Consider implementing authentication and authorization before exposing this endpoint in production environments.

## Integration with Custom Logger

This endpoint works seamlessly with the custom logger implementation in `Infrastructure/Logging/`. The logs are read from the same file that the custom logger writes to (`Logs/log.txt`).
