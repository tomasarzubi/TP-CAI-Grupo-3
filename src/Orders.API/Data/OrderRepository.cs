using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Data
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db");

        // Fila auxiliar: un item junto con el id de su orden
        private class ItemRow
        {
            public Guid OrderId { get; set; }
            public Guid ProductoId { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
        }

        private static OrderItem ToItem(ItemRow r) => new()
        {
            ProductoId = r.ProductoId,
            Cantidad = r.Cantidad,
            PrecioUnitario = r.PrecioUnitario
        };

        // ── GET ALL (filtro opcional ?usuarioId=) ──
        public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
        {
            using var conn = CreateConnection();
            var filtro = usuarioId?.ToString();

            var orders = (await conn.QueryAsync<Order>("""
                SELECT id, usuario_id AS UsuarioId, total, estado,
                fecha_creacion AS FechaCreacion
                FROM orders
                WHERE (@filtro IS NULL OR usuario_id = @filtro COLLATE NOCASE)
                ORDER BY fecha_creacion DESC
            """, new { filtro })).ToList();   // <- CAMBIO 1: COLLATE NOCASE

            var items = (await conn.QueryAsync<ItemRow>("""
                SELECT i.order_id AS OrderId, i.producto_id AS ProductoId,
                i.cantidad AS Cantidad, i.precio_unitario AS PrecioUnitario
                FROM order_items i
                JOIN orders o ON o.id = i.order_id
                WHERE (@filtro IS NULL OR o.usuario_id = @filtro COLLATE NOCASE)
                ORDER BY i.rowid
            """, new { filtro })).ToLookup(r => r.OrderId);   // <- CAMBIO 2: COLLATE NOCASE

            return orders.Select(o => o with { Items = items[o.Id].Select(ToItem).ToList() });
        }

        // ── GET BY ID ──
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            var order = await conn.QuerySingleOrDefaultAsync<Order>("""
                SELECT id, usuario_id AS UsuarioId, total, estado,
                fecha_creacion AS FechaCreacion
                FROM orders
                WHERE id = @id
            """, new { id });

            if (order is null) return null;

            var items = await conn.QueryAsync<ItemRow>("""
                SELECT order_id AS OrderId, producto_id AS ProductoId,
                cantidad AS Cantidad, precio_unitario AS PrecioUnitario
                FROM order_items
                WHERE order_id = @id
                ORDER BY rowid
            """, new { id });

            return order with { Items = items.Select(ToItem).ToList() };
        }

        // ── CREATE (orden + items en una transacción) ──
        public async Task<Order> CreateAsync(Guid usuarioId, List<OrderItem> items, decimal total)
        {
            var id = Guid.NewGuid();

            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            await conn.ExecuteAsync("""
                INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
                VALUES (@id, @usuarioId, @total, 'Pendiente', @fecha)
            """, new { id, usuarioId, total, fecha = DateTime.UtcNow }, tx);

            foreach (var item in items)
            {
                await conn.ExecuteAsync("""
                    INSERT INTO order_items (order_id, producto_id, cantidad, precio_unitario)
                    VALUES (@id, @ProductoId, @Cantidad, @PrecioUnitario)
                """, new { id, item.ProductoId, item.Cantidad, item.PrecioUnitario }, tx);
            }

            tx.Commit();
            return (await GetByIdAsync(id))!;
        }

        // ── UPDATE STATUS ──
        public async Task<bool> UpdateStatusAsync(Guid id, string estado)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "UPDATE orders SET estado = @estado WHERE id = @id", new { id, estado });
            return rows > 0;
        }
    }
}