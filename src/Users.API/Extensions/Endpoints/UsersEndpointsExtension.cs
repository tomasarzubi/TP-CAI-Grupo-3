using Products.API.DTOs;
using Products.API.Models;
using Products.API.Services;

namespace Products.API.Extensions.Endpoints
{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class ProductsEndpointsExtension
    {
        public static void MapProductEndpoints(this WebApplication app)
        {
            // GET all (filtros ?categoria= y ?nombre=)
            app.MapGet("/api/products", async (string? categoria, string? nombre, ProductService service) =>
            {
                var products = await service.GetAllAsync(categoria, nombre);
                return Results.Ok(products);
            })
            .WithTags("Products")
            .WithSummary("Listar productos")
            .Produces<IEnumerable<Product>>(200)
            .ProducesProblem(500);

            // GET by id
            app.MapGet("/api/products/{id}", async (Guid id, ProductService service) =>
            {
                var product = await service.GetByIdAsync(id);
                return Results.Ok(product);
            })
            .WithTags("Products")
            .WithSummary("Obtener producto por ID")
            .Produces<Product>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);

            // POST
            app.MapPost("/api/products", async (CreateProductRequest req, ProductService service) =>
            {
                var product = await service.CreateAsync(req);
                return Results.Created($"/api/products/{product.Id}", product);
            })
            .WithTags("Products")
            .WithSummary("Crear nuevo producto")
            .Produces<Product>(201)
            .ProducesProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

            // PUT
            app.MapPut("/api/products/{id}", async (Guid id, UpdateProductRequest req, ProductService service) =>
            {
                var product = await service.UpdateAsync(id, req);
                return Results.Ok(product);
            })
            .WithTags("Products")
            .WithSummary("Actualizar producto existente")
            .Produces<Product>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

            // DELETE
            app.MapDelete("/api/products/{id}", async (Guid id, ProductService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithTags("Products")
            .WithSummary("Eliminar producto")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500);
        }
    }
}
