using Cart.API.Clients;
using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using CartModel = Cart.API.Models.Cart;

namespace Cart.API.Services;

public class CartServices(CartRepository repo, ProductsClient products)
{
    public async Task<CartModel> GetAsync(Guid userId)
        => await repo.GetByUserIdAsync(userId)
           ?? throw new NotFoundException(ErrorCodes.CRT_001, "Carrito no encontrado.");

    public async Task<CartModel> AddItemAsync(Guid userId, AddCartItemRequest request)
    {
        RequestValidator.Validate(request, ErrorCodes.CRT_004);

        var product = await GetProductOrThrowAsync(request.ProductoId); // CRT-002

        // Si el producto ya está en el carrito la cantidad se suma: se valida el total
        var cart = await repo.GetByUserIdAsync(userId);
        var enCarrito = cart?.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId)?.Cantidad ?? 0;
        EnsureStock(product, enCarrito + request.Cantidad);              // CRT-003

        return await repo.AddItemAsync(userId, request.ProductoId, request.Cantidad);
    }

    public async Task<CartModel> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request)
    {
        RequestValidator.Validate(request, ErrorCodes.CRT_004);

        var cart = await GetAsync(userId); // lanza CRT-001 si no hay carrito
        EnsureItemInCart(cart, productId);

        var product = await GetProductOrThrowAsync(productId);           // CRT-002
        EnsureStock(product, request.Cantidad);                          // CRT-003

        await repo.UpdateItemAsync(userId, productId, request.Cantidad);
        return await GetAsync(userId);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        var cart = await GetAsync(userId); // lanza CRT-001 si no hay carrito
        EnsureItemInCart(cart, productId);

        await repo.RemoveItemAsync(userId, productId);
    }

    public async Task ClearAsync(Guid userId)
    {
        if (!await repo.ClearAsync(userId))
            throw new NotFoundException(ErrorCodes.CRT_001, "Carrito no encontrado.");
    }

    private async Task<ProductInfo> GetProductOrThrowAsync(Guid productId)
        => await products.GetByIdAsync(productId)
           ?? throw new NotFoundException(ErrorCodes.CRT_002, "Producto no encontrado.");

    private static void EnsureStock(ProductInfo product, int solicitado)
    {
        if (product.Stock < solicitado)
            throw new BusinessRuleException(ErrorCodes.CRT_003,
                $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {solicitado}.", 422);
    }

    private static void EnsureItemInCart(CartModel cart, Guid productId)
    {
        if (cart.Items.All(i => i.ProductoId != productId))
            throw new NotFoundException(ErrorCodes.CRT_002, "El producto no está en el carrito.");
    }
}