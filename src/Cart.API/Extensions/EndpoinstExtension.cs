//using Cart.API.Extensions.Endpoints;
using Cart.API.Models;
using Cart.API.Extensions.Endpoints;

namespace Cart.API.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        app.MapProductEndpoints();
    }
}

