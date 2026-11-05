using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services;

public class OrderService(OrderRepository repo, UsersClient users, ProductsClient products)
{
    private static readonly string[] Estados =
        ["Pendiente", "Confirmada", "Enviada", "Entregada", "Cancelada"];

    private static readonly Dictionary<string, string[]> Transiciones = new()
    {
        ["Pendiente"]  = ["Confirmada", "Cancelada"],
        ["Confirmada"] = ["Enviada", "Cancelada"],
        ["Enviada"]    = ["Entregada"],
        ["Entregada"]  = [],
        ["Cancelada"]  = []
    };

    public Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId) => repo.GetAllAsync(usuarioId);

    public async Task<Order> GetByIdAsync(Guid id)
    {
        var order = await repo.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada.");
        return order;
    }

    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        // Forma de la orden (lista requerida y no vacía) y datos de cada item
        RequestValidator.Validate(request, ErrorCodes.ORD_002);

        var errores = new List<string>();
        if (request.UsuarioId == Guid.Empty)
            errores.Add("El usuario es requerido.");
        if (request.Items.Any(i => i.ProductoId == Guid.Empty))
            errores.Add("Todos los items deben indicar un producto.");
        if (request.Items.Any(i => i.Cantidad <= 0))
            errores.Add("La cantidad de cada item debe ser mayor a 0.");
        if (errores.Count > 0)
            throw new ValidationException(ErrorCodes.ORD_002, string.Join("; ", errores));

        // El usuario debe existir en Users.API
        if (!await users.ExistsAsync(request.UsuarioId))
            throw new NotFoundException(ErrorCodes.ORD_003, "Usuario no encontrado.");

        // Cada producto debe existir en Products.API y tener stock
        var items = new List<OrderItem>();
        foreach (var grupo in request.Items.GroupBy(i => i.ProductoId))
        {
            var cantidad = grupo.Sum(i => i.Cantidad);

            var product = await products.GetProductAsync(grupo.Key);
            if (product == null)
                throw new NotFoundException(ErrorCodes.ORD_004, $"Producto no encontrado: {grupo.Key}.");

            if (cantidad > product.Stock)
                throw new BusinessRuleException(ErrorCodes.ORD_005,
                    $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {cantidad}.", 422);

            items.Add(new OrderItem
            {
                ProductoId = grupo.Key,
                Cantidad = cantidad,
                PrecioUnitario = product.Precio   // precio de este momento
            });
        }

        // Total calculado y alta (estado inicial: Pendiente)
        var total = items.Sum(i => i.PrecioUnitario * i.Cantidad);
        return await repo.CreateAsync(request.UsuarioId, items, total);
    }

    public async Task<UpdateOrderStatusResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        RequestValidator.Validate(request, ErrorCodes.ORD_002);

        if (!Estados.Contains(request.Estado))
            throw new ValidationException(ErrorCodes.ORD_002,
                $"Estado inválido: '{request.Estado}'. Valores posibles: {string.Join(", ", Estados)}.");

        var order = await GetByIdAsync(id); // lanza ORD-001 si no existe

        if (!Transiciones[order.Estado].Contains(request.Estado))
            throw new BusinessRuleException(ErrorCodes.ORD_006,
                $"Una orden en estado '{order.Estado}' no puede pasar a '{request.Estado}'.");

        await repo.UpdateStatusAsync(id, request.Estado);
        return new UpdateOrderStatusResponse(id, request.Estado, DateTime.UtcNow);
    }
}
