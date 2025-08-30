 
using InsuranceFraudDetection.Infrastructure.Logging;  
using System.Net;
using System.Text.Json; 

namespace InsuranceFraudDetection.Infrastructure.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICustomLogger _customLogger;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _fallbackLogger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ICustomLogger customLogger, ILogger<GlobalExceptionHandlerMiddleware> fallbackLogger)
        {
            _next = next;
            _customLogger = customLogger;
            _fallbackLogger = fallbackLogger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            } 
            catch (Exception ex)
            {
                await HandleUnhandledExceptionAsync(context, ex);
            }
        } 
   
        private async Task HandleUnhandledExceptionAsync(HttpContext context, Exception ex)
        { 
            await _customLogger.LogAsync(LogLevel.Error, $"Unhandled Exception: {ex.Message} | Path: {context.Request.Path} | Method: {context.Request.Method}", ex, callerMemberName: nameof(HandleUnhandledExceptionAsync));

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
             
            var errorResponse = new
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later.",
                Result = (object)null
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    // Extension method to register middleware
    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}