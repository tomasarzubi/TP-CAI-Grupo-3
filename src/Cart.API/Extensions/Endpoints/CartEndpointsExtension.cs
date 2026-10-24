using Cart.API.DTOs;
using Cart.API.Services;
using CartModel = Cart.API.Models.Cart;

namespace Cart.API.Extensions.Endpoints
{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class CartEndpointsExtension
    {
        public static void MapCartEndpoints(this WebApplication app)
        {
            // GET carrito del usuario
            app.MapGet("/api/cart/{userId}", async (Guid userId, CartServices service) =>
            {
                var cart = await service.GetAsync(userId);
                return Results.Ok(cart);
            })
            .WithTags("Cart")
            .WithSummary("Obtener carrito del usuario")
            .Produces<CartModel>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

            // POST agregar item (devuelve 200, no 201, según el contrato)
            app.MapPost("/api/cart/{userId}/items", async (Guid userId, AddCartItemRequest req, CartServices service) =>
            {
                var cart = await service.AddItemAsync(userId, req);
                return Results.Ok(cart);
            })
            .WithTags("Cart")
            .WithSummary("Agregar producto al carrito")
            .Produces<CartModel>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(500);

            // PUT actualizar cantidad de un item
            app.MapPut("/api/cart/{userId}/items/{productId}", async (Guid userId, Guid productId, UpdateCartItemRequest req, CartServices service) =>
            {
                var cart = await service.UpdateItemAsync(userId, productId, req);
                return Results.Ok(cart);
            })
            .WithTags("Cart")
            .WithSummary("Actualizar cantidad de un item")
            .Produces<CartModel>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(500);

            // DELETE quitar un item
            app.MapDelete("/api/cart/{userId}/items/{productId}", async (Guid userId, Guid productId, CartServices service) =>
            {
                await service.RemoveItemAsync(userId, productId);
                return Results.NoContent();
            })
            .WithTags("Cart")
            .WithSummary("Quitar un producto del carrito")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(500);

            // DELETE vaciar carrito
            app.MapDelete("/api/cart/{userId}", async (Guid userId, CartServices service) =>
            {
                await service.ClearAsync(userId);
                return Results.NoContent();
            })
            .WithTags("Cart")
            .WithSummary("Vaciar carrito completo")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(500);
        }
    }
}