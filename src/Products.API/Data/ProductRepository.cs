using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.DTOs;
using Products.API.Models;

namespace Products.API.Data
{
    public class ProductRepository
    {
        private readonly IConfiguration _config;

        public ProductRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=products.db");

        // ── GET ALL (con filtros opcionales ?categoria= y ?nombre=) ──
        public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<Product>("""
                SELECT id, nombre, descripcion, precio, stock, categoria,
                       fecha_creacion AS FechaCreacion
                FROM products
                WHERE (@categoria IS NULL OR categoria = @categoria)
                  AND (@nombre    IS NULL OR nombre LIKE '%' || @nombre || '%')
                ORDER BY fecha_creacion DESC
            """, new { categoria, nombre });
        }

        // ── GET BY ID ──
        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Product>("""
                SELECT id, nombre, descripcion, precio, stock, categoria,
                       fecha_creacion AS FechaCreacion
                FROM products
                WHERE id = @id
            """, new { id });
        }

        // ── EXISTE (nombre + categoría) → para PRD-003 ──
        public async Task<bool> ExistsByNombreCategoriaAsync(string nombre, string categoria)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>("""
                SELECT COUNT(1) FROM products
                WHERE LOWER(nombre) = LOWER(@nombre) AND categoria = @categoria
            """, new { nombre, categoria });
            return count > 0;
        }

        // ── CREATE ──
        public async Task<Product> CreateAsync(CreateProductRequest request)
        {
            using var conn = CreateConnection();
            var id = Guid.NewGuid();

            await conn.ExecuteAsync("""
                INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion)
                VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
            """, new
            {
                Id = id,
                request.Nombre,
                request.Descripcion,
                request.Precio,
                request.Stock,
                request.Categoria,
                FechaCreacion = DateTime.UtcNow
            });

            return (await GetByIdAsync(id))!;
        }

        // ── UPDATE ──
        public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            using var conn = CreateConnection();

            var rows = await conn.ExecuteAsync("""
                UPDATE products
                SET nombre      = @Nombre,
                    descripcion = @Descripcion,
                    precio      = @Precio,
                    stock       = @Stock,
                    categoria   = @Categoria
                WHERE id = @Id
            """, new { request.Nombre, request.Descripcion, request.Precio, request.Stock, request.Categoria, Id = id });

            return rows > 0;
        }

        // ── DELETE ──
        public async Task<bool> DeleteAsync(Guid id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM products WHERE id = @id", new { id });
            return rows > 0;
        }
    }
}
