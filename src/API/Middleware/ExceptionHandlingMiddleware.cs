using Domain.Exceptions;
using FluentValidation;

namespace API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled request error {TraceId}", context.TraceIdentifier);

            var status = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/" + status,
                title = status == StatusCodes.Status500InternalServerError && !environment.IsDevelopment() ? "Unexpected server error" : ex.Message,
                status,
                traceId = context.TraceIdentifier,
                errors = ex is ValidationException validationEx
                    ? validationEx.Errors.Select(x => new { x.PropertyName, x.ErrorMessage })
                    : null
            });
        }
    }
}
