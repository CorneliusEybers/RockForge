using RockForge.API.ExceptionHandling;
using RockForge.API.Middleware;
using RockForge.Application.RockService;
using RockForge.Application.Validation.Strategies;
using System.Text.Json.Serialization;

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
builder.Services.AddSwaggerGen();

// Category validation strategies
builder.Services.AddSingleton<IRockValidationStrategy, RevenueRockValidationStrategy>();
builder.Services.AddSingleton<IRockValidationStrategy, HealthRockValidationStrategy>();
builder.Services.AddSingleton<IRockValidationStrategy, CareerRockValidationStrategy>();
builder.Services.AddSingleton<IRockValidationStrategy, OtherRockValidationStrategy>();

// - Service registrations
builder.Services.AddSingleton<IRockService, RockService>();

// - Structured Logging Correlation
builder.Logging.ClearProviders();

builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});


// - Run the Application
var app = builder.Build();

// - Structured Logging Correlation
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// - Centralized Exception Handling
app.UseExceptionHandler();

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
