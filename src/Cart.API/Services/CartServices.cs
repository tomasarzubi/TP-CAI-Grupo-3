using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using CartModel = Cart.API.Models.Cart;

namespace Cart.API.Services;

public class CartServices(CartRepository repo)
{
    public async Task<CartModel> GetAsync(Guid userId)
        => await repo.GetByUserIdAsync(userId)
           ?? throw new NotFoundException(ErrorCodes.CRT_001, "Carrito no encontrado.");

    public async Task<CartModel> AddItemAsync(Guid userId, AddCartItemRequest request)
    {
        RequestValidator.Validate(request, ErrorCodes.CRT_004);

        return await repo.AddItemAsync(userId, request.ProductoId, request.Cantidad);
    }

    public async Task<CartModel> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request)
    {
        RequestValidator.Validate(request, ErrorCodes.CRT_004);

        var cart = await GetAsync(userId); // lanza CRT-001 si no hay carrito
        EnsureItemInCart(cart, productId);

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

    private static void EnsureItemInCart(CartModel cart, Guid productId)
    {
        if (cart.Items.All(i => i.ProductoId != productId))
            throw new NotFoundException(ErrorCodes.CRT_002, "El producto no está en el carrito.");
    }
}
