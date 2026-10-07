using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RockForge.Application.Exceptions;

namespace RockForge.API.ExceptionHandling
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not RockValidationException validationException)
            {
                return false;
            }

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = validationException.Message,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            httpContext.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }
    }
}