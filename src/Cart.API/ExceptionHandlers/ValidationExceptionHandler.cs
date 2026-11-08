using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException ex) return false;

        logger.LogWarning("{ErrorCode} {ErrorMessage} {Endpoint} {CorrelationId}",
            ex.ErrorCode, ex.Message, context.Request.Path.Value, context.TraceIdentifier);

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "Los datos enviados no son válidos.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId = context.TraceIdentifier
        }, cancellationToken: cancellationToken);
        return true;
    }
}
