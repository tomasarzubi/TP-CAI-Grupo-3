//using Users.API.Extensions.Endpoints;
using Users.API.Models;
using Users.API.Extensions.Endpoints;

namespace Users.API.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        app.MapUserEndpoints();
    }
}
