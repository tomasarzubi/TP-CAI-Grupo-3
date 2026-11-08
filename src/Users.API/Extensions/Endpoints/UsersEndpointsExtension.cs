using Users.API.DTOs;
using Users.API.Models;
using Users.API.Exceptions;
using Users.API.Services;

namespace Users.API.Extensions.Endpoints
{
    public static class UsersEndpointsExtension
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            // POST /register
            app.MapPost("/api/users/register", async (RegisterUserRequest req, UserService service) =>
            {
                RequestValidator.Validate(req, ErrorCodes.USR_002);              
                var user = await service.RegisterAsync(req);
                return Results.Created($"/api/users/{user.Id}", user);
            })
            .WithTags("Users")
            .WithSummary("Registrar nuevo usuario")
            .Produces<User>(201)
            .ProducesProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

            // POST /login
            app.MapPost("/api/users/login", async (LoginUserRequest req, UserService service) =>
            {
                RequestValidator.Validate(req, ErrorCodes.USR_002);               
                var user = await service.LoginAsync(req);
                return Results.Ok(user);
            })
            .WithTags("Users")
            .WithSummary("Autenticar usuario")
            .Produces<User>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(500);

            //  lo usan Orders y Notifications para validar que exista el usuario y que este activo
            app.MapGet("/api/users/{id}", async (Guid id, UserService service) =>
            {
                var u = await service.GetByIdAsync(id);
                
                if (u == null)
                {
                    return Results.NotFound(new { message = "Usuario no encontrado." });
                }

                return Results.Ok(new { u.Id, u.Nombre, u.Apellido, u.Email, u.FechaRegistro, u.Activo });
            })
            .WithTags("Users")
            .WithSummary("Obtener usuario por ID")
            .Produces(200)
            .Produces(404)
            .ProducesProblem(500);
        }
    }
}
