using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace InsuranceFraudDetection.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public HealthController(IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
        }
 

        [HttpPost("shutdown")]
        public ActionResult<string> ShutdownServer()
        { 
            _hostApplicationLifetime.StopApplication();
            
            return Ok("Server shutdown initiated");
        }
    }
}
