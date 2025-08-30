using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InsuranceFraudDetection.WebAPI.Models;

namespace InsuranceFraudDetection.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IHostApplicationLifetime hostApplicationLifetime, ILogger<HealthController> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
        }
 

        [HttpGet("shutdown")]
        public ActionResult<string> ShutdownServer()
        { 
            _hostApplicationLifetime.StopApplication(); 
            
            return Ok();
        }

        [HttpGet("logs")]
        public ActionResult<LogsResponse> GetLogs()
        {
            try
            {
                _logger.LogInformation("Retrieving application logs");
                
                var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");
                
                if (!System.IO.File.Exists(logFilePath))
                {
                    _logger.LogWarning("Log file not found at path: {LogFilePath}", logFilePath);
                    return NotFound(new LogsResponse 
                    { 
                        Success = false, 
                        Message = "Log file not found",
                        Logs = new List<string>(),
                        TotalLines = 0
                    });
                }

                var logLines = System.IO.File.ReadAllLines(logFilePath);
                var logs = logLines.ToList();

                _logger.LogInformation("Successfully retrieved {LogCount} log entries", logs.Count); 
                string allLogs = logs.Count > 0 ? string.Join(Environment.NewLine, logs) : "No logs available";
                return Ok(allLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving logs");
                return StatusCode(500, new LogsResponse
                {
                    Success = false,
                    Message = $"Error retrieving logs: {ex.Message}",
                    Logs = new List<string>(),
                    TotalLines = 0
                });
            }
        }
    }
}
