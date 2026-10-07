using System.Diagnostics;

namespace RockForge.API.Middleware
{
    public sealed class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogDebug("HTTP request started {Method} {Path}",
                             httpContext.Request.Method,
                             httpContext.Request.Path);

            try
            {
                await _next(httpContext);
            }
            finally
            {
                stopwatch.Stop();

                LogRequestOutcome(httpContext, stopwatch.ElapsedMilliseconds);
            }
        }

        private void LogRequestOutcome(HttpContext httpContext, long elapsedMilliseconds)
        {
            var statusCode = httpContext.Response.StatusCode;

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError("HTTP request completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms",
                                 httpContext.Request.Method,
                                 httpContext.Request.Path,
                                 statusCode,
                                 elapsedMilliseconds);

                return;
            }

            if (statusCode >= StatusCodes.Status400BadRequest)
            {
                _logger.LogWarning("HTTP request completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms",
                                   httpContext.Request.Method,
                                   httpContext.Request.Path,
                                   statusCode,
                                   elapsedMilliseconds);

                return;
            }

            _logger.LogInformation("HTTP request completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms",
                                   httpContext.Request.Method,
                                   httpContext.Request.Path,
                                   statusCode,
                                   elapsedMilliseconds);
        }
    }
}