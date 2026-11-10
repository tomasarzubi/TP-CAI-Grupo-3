using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

// class ProductService(ProductRepository repo)

public class ProductService(ProductRepository repo, OrdersClient orders)
{
    public Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
        => repo.GetAllAsync(categoria, nombre);

    public async Task<Product> GetByIdAsync(Guid id)
    {
        var product = await repo.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException(ErrorCodes.PRD_001, "Producto no encontrado.");
        return product;
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        RequestValidator.Validate(request, ErrorCodes.PRD_002);

        if (await repo.ExistsByNombreCategoriaAsync(request.Nombre, request.Categoria))
            throw new BusinessRuleException(ErrorCodes.PRD_003,
                $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

        return await repo.CreateAsync(request);
    }

    public async Task<Product> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        RequestValidator.Validate(request, ErrorCodes.PRD_002);

        var updated = await repo.UpdateAsync(id, request);
        if (!updated)
            throw new NotFoundException(ErrorCodes.PRD_001, "Producto no encontrado.");

        return (await repo.GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetByIdAsync(id); // lanza PRD-001 si no existe

        if (await orders.TieneOrdenesActivasAsync(id))
    throw new BusinessRuleException(ErrorCodes.PRD_004,
        "El producto tiene órdenes activas y no puede eliminarse.");

        await repo.DeleteAsync(id);
    }
}
