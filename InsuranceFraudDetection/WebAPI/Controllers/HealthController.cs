using InsuranceFraudDetection.Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace InsuranceFraudDetection.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ICustomLogger logger;

        public HealthController(IHostApplicationLifetime hostApplicationLifetime, ICustomLogger loggerr)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            logger = loggerr;
        }


        [HttpPost("shutdown")]
        public ActionResult<string> ShutdownServer()
        {
            _hostApplicationLifetime.StopApplication();

            return Ok("Server shutdown initiated");
        }

        [HttpPost("logs")]
        public async Task<ActionResult> logs()
        {
            var logs= await logger.GetLogs();
            return Ok(logs);
        }
    }
}
