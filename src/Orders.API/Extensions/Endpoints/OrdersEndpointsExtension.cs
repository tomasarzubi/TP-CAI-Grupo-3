using Orders.API.DTOs;
using Orders.API.Models;
using Orders.API.Services;

namespace Orders.API.Extensions.Endpoints
{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class OrdersEndpointsExtension
    {
        public static void MapOrderEndpoints(this WebApplication app)
        {
            // GET all (filtro ?usuarioId=)
            app.MapGet("/api/orders", async (Guid? usuarioId, OrderService service) =>
            {
                var orders = await service.GetAllAsync(usuarioId);
                return Results.Ok(orders);
            })
            .WithTags("Orders")
            .WithSummary("Listar órdenes")
            .Produces<IEnumerable<Order>>(200)
            .ProducesProblem(500);

            // GET by id
            app.MapGet("/api/orders/{id}", async (Guid id, OrderService service) =>
            {
                var order = await service.GetByIdAsync(id);
                return Results.Ok(order);
            })
            .WithTags("Orders")
            .WithSummary("Obtener detalle de una orden")
            .Produces<Order>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

            // POST
            app.MapPost("/api/orders", async (CreateOrderRequest req, OrderService service) =>
            {
                var order = await service.CreateAsync(req);
                return Results.Created($"/api/orders/{order.Id}", order);
            })
            .WithTags("Orders")
            .WithSummary("Crear nueva orden")
            .Produces<Order>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(500);

            // PUT status
            app.MapPut("/api/orders/{id}/status", async (Guid id, UpdateOrderStatusRequest req, OrderService service) =>
            {
                var result = await service.UpdateStatusAsync(id, req);
                return Results.Ok(result);
            })
            .WithTags("Orders")
            .WithSummary("Actualizar estado de la orden")
            .Produces<UpdateOrderStatusResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500);
        }
    }
}
