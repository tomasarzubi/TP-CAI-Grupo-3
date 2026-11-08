using Cart.API.Middleware;

namespace Cart.API.Clients;

public record ProductInfo
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
}

public class ProductsClient(HttpClient http)
{
    // Devuelve null si Products responde 404. Cualquier otro error HTTP lanza
    // HttpRequestException, que el GlobalExceptionHandler convierte en 500 / CRT-005.
    public async Task<ProductInfo?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await http.GetAsync($"/api/products/{id}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductInfo>(ct);
    }
}

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