using Microsoft.OpenApi;
using RockForge.API.ExceptionHandling;
using RockForge.API.Middleware;
using RockForge.Application.RockService;
using RockForge.Application.Validation.Strategies;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using RockForge.Infrastructure.ProfileClient;
using RockForge.Application.ProfileService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
                .AddJsonOptions(options => {
                                               options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                                           });

// - Centralized Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// - Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>{
                                            const string apiKeyScheme = "ApiKey";

                                            options.AddSecurityDefinition(apiKeyScheme, new OpenApiSecurityScheme
                                            {
                                                Description = "Enter the API key",
                                                Type = SecuritySchemeType.ApiKey,
                                                Name = "X-Api-Key",
                                                In = ParameterLocation.Header
                                            });

                                            options.AddSecurityRequirement(document =>
                                                new OpenApiSecurityRequirement
                                                {
                                                    [new OpenApiSecuritySchemeReference(apiKeyScheme, document)] = []
                                                });
                                         });

// Category validation strategies
builder.Services.AddSingleton<IRockValidationStrategy, RevenueRockValidationStrategy>();
builder.Services.AddSingleton<IRockValidationStrategy, HealthRockValidationStrategy>();
builder.Services.AddSingleton<IRockValidationStrategy, CareerRockValidationStrategy>();
builder.Services.AddSingleton<IRockValidationStrategy, OtherRockValidationStrategy>();

// - Service registrations
builder.Services.AddSingleton<IRockService, RockService>();
builder.Services.AddTransient<IEnrichedProfileService, EnrichedProfileService>();

// - Structured Logging Correlation
builder.Logging.ClearProviders();

builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});

// - Profile Integration Resilience
builder.Services.AddHttpClient<IProfileClient, JsonPlaceholderProfileClient>(client => {
                                                                                           client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");

                                                                                           client.Timeout = TimeSpan.FromSeconds(10);
                                                                                       })
                .AddResilienceHandler(
                    "ProfileClientResilience",
                    (resilienceBuilder, context) =>
                    {
                        var loggerFactory =
                            context.ServiceProvider
                                   .GetRequiredService<ILoggerFactory>();

                        var logger =
                            loggerFactory.CreateLogger("ProfileClientRetry");

                        resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = 3,
                            Delay = TimeSpan.FromSeconds(1),
                            BackoffType = DelayBackoffType.Exponential,
                            UseJitter = true,
                            OnRetry = args =>
                            {
                                logger.LogWarning(
                                    "Retrying profile request. Attempt {AttemptNumber}, Delay {Delay}, Reason {Reason}",
                                    args.AttemptNumber + 1,
                                    args.RetryDelay,
                                    args.Outcome.Exception?.Message
                                        ?? args.Outcome.Result?.StatusCode.ToString()
                                        ?? "Unknown");

                                return ValueTask.CompletedTask;
                            }
                        });

                        resilienceBuilder.AddTimeout(
                            TimeSpan.FromSeconds(10));
                    });


// - Run the Application
var app = builder.Build();

// - Structured Logging Correlation
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// - Centralized Exception Handling
app.UseExceptionHandler();

// - Api Key Authentication
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
