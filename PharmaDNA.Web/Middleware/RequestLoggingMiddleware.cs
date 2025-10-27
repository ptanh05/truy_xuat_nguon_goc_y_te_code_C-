using System.Diagnostics;

namespace PharmaDNA.Web.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];
            
            // Add request ID to response headers
            context.Response.Headers.Add("X-Request-ID", requestId);
            
            // Log request
            _logger.LogInformation(
                "Request {RequestId} started: {Method} {Path} from {RemoteIp}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
            );

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Request {RequestId} failed: {Method} {Path} - {Error}",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    ex.Message
                );
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                _logger.LogInformation(
                    "Request {RequestId} completed: {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
