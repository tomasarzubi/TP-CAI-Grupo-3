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
return Results.Created($"/api/users/{user.Id}",
    new UserResponse(user.Id, user.Nombre, user.Apellido, user.Email, user.FechaRegistro, user.Activo));
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
            return Results.Ok(
                new UserResponse(user.Id, user.Nombre, user.Apellido, user.Email, user.FechaRegistro, user.Activo));
            })
            .WithTags("Users")
            .WithSummary("Autenticar usuario")
            .Produces<User>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(500);

                        // GET /api/users/{id}
            app.MapGet("/api/users/{id:guid}", async (Guid id, UserService service) =>
            {
                var user = await service.GetByIdAsync(id);

                if (user == null)
                    return Results.NotFound();

                return Results.Ok(user);
            })
            .WithTags("Users")
            .WithSummary("Buscar usuario por ID")
            .Produces<User>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);
        }
    }
}
