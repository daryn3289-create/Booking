using Booking.Shared.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Shared.Api;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var isDomainException = exception is DomainException;
        var statusCode = isDomainException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        var title = isDomainException ? "A domain error occurred." : "An unexpected error occurred.";

        var problemDetails = ProblemDetailsFactory.Create(
            title: title,
            detail: exception.Message,
            status: statusCode,
            instance: httpContext.Request.Path,
            details: new Dictionary<string, object?>
            {
                { "ExceptionType", exception.GetType().FullName ?? "Unknown" },
                { "StackTrace", exception.StackTrace ?? "No stack trace available" }
            });

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

public static class ProblemDetailsFactory
{
    public static ProblemDetails Create(string title, string detail, int status, string instance, Dictionary<string, object?> details)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status,
            Instance = instance,
            Extensions = details
        };
    }
}

