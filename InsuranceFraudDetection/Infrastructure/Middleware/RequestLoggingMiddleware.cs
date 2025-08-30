 
using InsuranceFraudDetection.Infrastructure.Logging; 
using System.Diagnostics; 
using System.Text; 

namespace InsuranceFraudDetection.Infrastructure.Middleware 
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICustomLogger _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ICustomLogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var requestInfo = await GetRequestInfo(context);

            await _logger.LogAsync(LogLevel.Trace, requestInfo);


            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Error, $"Exception during request processing: {ex.Message} | Path: {context.Request.Path} | Method: {context.Request.Method}", ex, callerMemberName: nameof(InvokeAsync));
           
                throw;
            }
            finally
            {
                stopwatch.Stop();
                await _logger.LogAsync(LogLevel.Trace, $"Response: {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms");

            }
        }

        private async Task<string> GetRequestInfo(HttpContext context)
        {
            var request = context.Request;
            var sb = new StringBuilder();

            sb.Append($"Request: {request.Method} {request.Path} from {context.Connection.RemoteIpAddress}");
             
            if (request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                if (request.QueryString.HasValue)
                {
                    sb.Append($" | Query: {request.QueryString}");
                }
            } 
            else if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                var body = await GetRequestBody(context);
                if (!string.IsNullOrEmpty(body))
                {
                    sb.Append($" | Body: {body}");
                }
            }

            return sb.ToString();
        }

        private async Task<string> GetRequestBody(HttpContext context)
        {
            try
            { 
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

                var body = await reader.ReadToEndAsync(); 
                context.Request.Body.Position = 0;

                return body;
            }
            catch (Exception ex)
            {
                return $"Error reading request body: {ex.Message}";
            }
        }
    }
     
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}