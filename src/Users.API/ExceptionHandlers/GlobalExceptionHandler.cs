using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        // [NUEVO] Body mal formado o parámetro de ruta con tipo incorrecto: es 400, no 500
        if (exception is BadHttpRequestException)
        {
            logger.LogWarning("{ErrorCode} {Endpoint} {CorrelationId}",
                ErrorCodes.PRD_002, context.Request.Path.Value, context.TraceIdentifier);

            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos enviados no son válidos.",
                instance = context.Request.Path.Value,
                errorCode = ErrorCodes.PRD_002,
                errorMessage = "Los datos del producto son inválidos.",
                correlationId = context.TraceIdentifier
            }, cancellationToken: cancellationToken);
            return true;
        }

        // Error inesperado: se loguea con el stack, pero no se expone al cliente
        logger.LogError(exception, "{ErrorCode} {Endpoint} {CorrelationId}",
            ErrorCodes.PRD_005, context.Request.Path.Value, context.TraceIdentifier);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = env.IsDevelopment() ? exception.Message : "Ocurrió un error interno.",
            instance = context.Request.Path.Value,
            errorCode = ErrorCodes.PRD_005,
            errorMessage = "Error interno al procesar el producto.",
            correlationId = context.TraceIdentifier
        }, cancellationToken: cancellationToken);
        return true;
    }
}
