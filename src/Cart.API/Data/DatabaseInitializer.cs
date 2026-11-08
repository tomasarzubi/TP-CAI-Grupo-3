
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            IConfiguration config,
            ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            var connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS carts (
                    id TEXT PRIMARY KEY,
                    user_id TEXT NOT NULL UNIQUE,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );
                """);

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS cart_items (
                    id TEXT PRIMARY KEY,
                    cart_id TEXT NOT NULL,
                    product_id TEXT NOT NULL,
                    quantity INTEGER NOT NULL CHECK (quantity > 0),
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL,
                    FOREIGN KEY (cart_id)
                        REFERENCES carts(id)
                        ON DELETE CASCADE,
                    UNIQUE (cart_id, product_id)
                );
                """);

            _logger.LogInformation(
                "SQLite del carrito inicializado correctamente → {db}",
                connectionString);
        }
    }
}
