namespace RockForge.API.Middleware
{
    public sealed class ApiKeyAuthenticationMiddleware
    {
        private const string ApiKeyHeader = "X-Api-Key";

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationMiddleware(RequestDelegate next,
                                              IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(httpContext);
                return;
            }

            var configuredApiKey = _configuration["Authentication:ApiKey"];

            if (string.IsNullOrWhiteSpace(configuredApiKey))
            {
                throw new InvalidOperationException("API key configuration is missing.");
            }

            if (!httpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var suppliedApiKey) 
                ||
                suppliedApiKey != configuredApiKey)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await httpContext.Response.WriteAsJsonAsync(new {
                                                                    title = "Unauthorized",
                                                                    status = StatusCodes.Status401Unauthorized,
                                                                    detail = "A valid API key is required."
                                                                });

                return;
            }

            await _next(httpContext);
        }
    }
}