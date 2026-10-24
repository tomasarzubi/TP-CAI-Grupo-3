
using Dapper;
using Microsoft.Data.Sqlite;
using Cart.API.Models;

namespace Cart.API.Data;

public class CartRepository
{
    private readonly IConfiguration _config;

    public CartRepository(IConfiguration config)
    {
        _config = config;
    }

    private SqliteConnection CreateConnection() =>
        new(_config.GetConnectionString("DefaultConnection")
            ?? "Data Source=cart.db");

    // Consultar el carrito de un usuario.
    public async Task<Cart.API.Models.Cart?> GetByUserIdAsync(Guid usuarioId)
    {
        using var conn = CreateConnection();

        var cart = await conn.QuerySingleOrDefaultAsync<Cart.API.Models.Cart>("""
            SELECT
                user_id AS UsuarioId,
                updated_at AS FechaActualizacion
            FROM carts
            WHERE user_id = @usuarioId
            """, new { usuarioId });

        if (cart is null)
            return null;

        var items = await conn.QueryAsync<CartItem>("""
            SELECT
                product_id AS ProductoId,
                quantity AS Cantidad
            FROM cart_items
            WHERE cart_id = (
                SELECT id FROM carts WHERE user_id = @usuarioId
            )
            ORDER BY created_at
            """, new { usuarioId });

        return cart with { Items = items.ToList() };
    }

    // Agregar un producto. Si el carrito no existe, lo crea.
    // Si el producto ya está en el carrito, suma la cantidad.
    public async Task<Cart.API.Models.Cart> AddItemAsync(
        Guid usuarioId,
        Guid productoId,
        int cantidad)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        using var transaction = conn.BeginTransaction();

        var ahora = DateTime.UtcNow.ToString("o");

        var cartId = await conn.QuerySingleOrDefaultAsync<string>("""
            SELECT id
            FROM carts
            WHERE user_id = @usuarioId
            """, new { usuarioId }, transaction);

        if (cartId is null)
        {
            cartId = Guid.NewGuid().ToString();

            await conn.ExecuteAsync("""
                INSERT INTO carts (id, user_id, created_at, updated_at)
                VALUES (@cartId, @usuarioId, @ahora, @ahora)
                """, new { cartId, usuarioId, ahora }, transaction);
        }

        await conn.ExecuteAsync("""
            INSERT INTO cart_items
                (id, cart_id, product_id, quantity, created_at, updated_at)
            VALUES
                (@id, @cartId, @productoId, @cantidad, @ahora, @ahora)
            ON CONFLICT (cart_id, product_id)
            DO UPDATE SET
                quantity = cart_items.quantity + excluded.quantity,
                updated_at = excluded.updated_at
            """,
            new
            {
                id = Guid.NewGuid().ToString(),
                cartId,
                productoId,
                cantidad,
                ahora
            },
            transaction);

        await conn.ExecuteAsync("""
            UPDATE carts
            SET updated_at = @ahora
            WHERE id = @cartId
            """, new { ahora, cartId }, transaction);

        transaction.Commit();

        return (await GetByUserIdAsync(usuarioId))!;
    }

    // Modificar la cantidad de un producto existente.
    public async Task<bool> UpdateItemAsync(
        Guid usuarioId,
        Guid productoId,
        int cantidad)
    {
        using var conn = CreateConnection();

        var ahora = DateTime.UtcNow.ToString("o");

        var rows = await conn.ExecuteAsync("""
            UPDATE cart_items
            SET quantity = @cantidad,
                updated_at = @ahora
            WHERE product_id = @productoId
              AND cart_id = (
                  SELECT id FROM carts WHERE user_id = @usuarioId
              )
            """, new { usuarioId, productoId, cantidad, ahora });

        if (rows > 0)
        {
            await conn.ExecuteAsync("""
                UPDATE carts
                SET updated_at = @ahora
                WHERE user_id = @usuarioId
                """, new { usuarioId, ahora });
        }

        return rows > 0;
    }

    // Eliminar un producto del carrito.
    public async Task<bool> RemoveItemAsync(
        Guid usuarioId,
        Guid productoId)
    {
        using var conn = CreateConnection();

        var rows = await conn.ExecuteAsync("""
            DELETE FROM cart_items
            WHERE product_id = @productoId
              AND cart_id = (
                  SELECT id FROM carts WHERE user_id = @usuarioId
              )
            """, new { usuarioId, productoId });

        if (rows > 0)
        {
            var ahora = DateTime.UtcNow.ToString("o");

            await conn.ExecuteAsync("""
                UPDATE carts
                SET updated_at = @ahora
                WHERE user_id = @usuarioId
                """, new { usuarioId, ahora });
        }

        return rows > 0;
    }

    // Vaciar el carrito sin eliminar el carrito del usuario.
    public async Task<bool> ClearAsync(Guid usuarioId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        var cartId = await conn.QuerySingleOrDefaultAsync<string>("""
            SELECT id
            FROM carts
            WHERE user_id = @usuarioId
            """, new { usuarioId });

        if (cartId is null)
            return false;

        using var transaction = conn.BeginTransaction();

        await conn.ExecuteAsync("""
            DELETE FROM cart_items
            WHERE cart_id = @cartId
            """, new { cartId }, transaction);

        var ahora = DateTime.UtcNow.ToString("o");

        await conn.ExecuteAsync("""
            UPDATE carts
            SET updated_at = @ahora
            WHERE id = @cartId
            """, new { ahora, cartId }, transaction);

        transaction.Commit();

        return true;
    }
}
