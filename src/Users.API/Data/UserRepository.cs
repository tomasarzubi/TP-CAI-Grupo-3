using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data
{
    public class UserRepository
    {
        private readonly IConfiguration _config;
        public UserRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=users.db");

        // Sirve para el Login y para verificar si el email ya existe
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
                SELECT id AS Id, nombre AS Nombre, apellido AS Apellido, email AS Email,
                       password_hash AS PasswordHash, fecha_registro AS FechaRegistro,
                       activo AS Activo, intentos_fallidos AS IntentosFallidos
                FROM users
                WHERE email = @email
            """, new { email });
        }

        // Sirve para el Register
        public async Task<User> CreateAsync(User user)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
                VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos)
            """, user);
            return user;
        }
       // Sirve para consultar un usuario por ID (usado por Orders y Notifications)
        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
                SELECT id AS Id, nombre AS Nombre, apellido AS Apellido, email AS Email,
                       password_hash AS PasswordHash, fecha_registro AS FechaRegistro,
                       activo AS Activo, intentos_fallidos AS IntentosFallidos
                FROM users
                WHERE id = @id
            """, new { id });
        }
        // Sirve para bloquear la cuenta sumando intentos fallidos
        public async Task<bool> UpdateAsync(User user)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("""
                UPDATE users
                SET activo = @Activo, intentos_fallidos = @IntentosFallidos
                WHERE id = @Id
            """, user);
            return rows > 0;
        }
    }
}