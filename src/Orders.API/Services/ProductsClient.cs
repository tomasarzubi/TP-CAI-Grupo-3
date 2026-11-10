using System.Net;
using System.Net.Http.Json;

namespace Orders.API.Services;

public record ProductInfo(Guid Id, string Nombre, decimal Precio, int Stock);

public class ProductsClient(HttpClient http)
{
    public async Task<ProductInfo?> GetProductAsync(Guid productoId)
    {
        var response = await http.GetAsync($"/api/products/{productoId}");

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductInfo>();
    }
}
