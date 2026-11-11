using Orders.API.Middleware;

namespace Orders.API.Services;

// Propaga el X-Correlation-Id a las llamadas salientes
public class CorrelationIdHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var id = accessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(id))
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, id);

        return base.SendAsync(request, ct);
    }
}