namespace RockForge.API.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-Id";
        private const string CorrelationIdItem = "CorrelationId";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next,
                                       ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var suppliedCorrelationId = httpContext.Request.Headers[CorrelationIdHeader].FirstOrDefault();

            var correlationId = string.IsNullOrWhiteSpace(suppliedCorrelationId)
                                ? Guid.NewGuid().ToString("N")
                                : suppliedCorrelationId;

            httpContext.Items[CorrelationIdItem] = correlationId;

            // Return the same ID to the subscriber.
            httpContext.Response.Headers[CorrelationIdHeader] = correlationId;

            using (_logger.BeginScope(new Dictionary<string, object> {["CorrelationId"] = correlationId }))
            {
                await _next(httpContext);
            }
        }
    }
}