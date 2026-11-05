//using Orders.API.Extensions.Endpoints;
using Orders.API.Models;
using Orders.API.Extensions.Endpoints;

namespace Orders.API.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        app.MapOrderEndpoints();
    }
}

