using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex) return false;

        logger.LogWarning("{ErrorCode} {ErrorMessage} {Endpoint} {CorrelationId}",
            ex.ErrorCode, ex.Message, context.Request.Path.Value, context.TraceIdentifier);

        var (type, title, detail) = ex.StatusCode switch
        {
            422 => ("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
            400 => ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "La solicitud no es válida."),
            _   => ("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede completar la operación por un conflicto.")
        };

        context.Response.StatusCode = ex.StatusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            type,
            title,
            status = ex.StatusCode,
            detail,
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId = context.TraceIdentifier
        }, cancellationToken: cancellationToken);
        return true;
    }
}
