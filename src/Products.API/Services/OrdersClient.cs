using System.Net.Http.Json;

namespace Products.API.Services;

public record OrderItemInfo(Guid ProductoId);
public record OrderInfo(Guid Id, string Estado, List<OrderItemInfo> Items);

public class OrdersClient(HttpClient http)
{
    // Devuelve true si hay alguna orden Pendiente o Confirmada con ese producto
    public async Task<bool> TieneOrdenesActivasAsync(Guid productoId)
    {
        var response = await http.GetAsync("/api/orders");
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderInfo>>();
        if (orders == null)
            return false;

        foreach (var orden in orders)
        {
            if (orden.Estado != "Pendiente" && orden.Estado != "Confirmada")
                continue;

            foreach (var item in orden.Items)
            {
                if (item.ProductoId == productoId)
                    return true;
            }
        }

        return false;
    }
}