using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RockForge.Application.Exceptions;
using RockForge.Domain.Exceptions;

namespace RockForge.API.ExceptionHandling
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                    Exception exception,
                                                    CancellationToken cancellationToken)
        {
            var problemDetails = exception switch
            {
                RockValidationException validationException => CreateProblemDetails(httpContext,
                                                                                    StatusCodes.Status400BadRequest,
                                                                                    "Validation failed",
                                                                                    validationException.Message),
                RockNotFoundException notFoundException => CreateProblemDetails(httpContext,
                                                                                StatusCodes.Status404NotFound,
                                                                                "Rock not found",
                                                                                notFoundException.Message),
                InvalidRockStateTransitionException transitionException => CreateProblemDetails(httpContext,
                                                                                                StatusCodes.Status422UnprocessableEntity,
                                                                                                "Invalid Rock state transition",
                                                                                                transitionException.Message),

                                                                                                _ =>
                                                                           CreateProblemDetails(httpContext,
                                                                                                StatusCodes.Status500InternalServerError,
                                                                                                "An unexpected error occurred",
                                                                                                "An unexpected error occurred while processing the request.")
            };

            if (problemDetails.Status ==
                StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception,
                                 "Unhandled exception occurred while processing request {RequestPath}",
                                 httpContext.Request.Path);
            }
            else
            {
                _logger.LogWarning(exception,
                                   "Request failed with status code {StatusCode} for {RequestPath}",
                                   problemDetails.Status,
                                   httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = problemDetails.Status!.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails,
                                                        cancellationToken);

            return true;
        }

        private static ProblemDetails CreateProblemDetails(HttpContext httpContext,
                                                           int statusCode,
                                                           string title,
                                                           string detail)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId?.ToString();
            }

            return problemDetails;
        }
    }
}