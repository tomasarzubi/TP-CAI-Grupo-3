using Cart.API.Extensions.Endpoints;

namespace Cart.API.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapCartEndpoints();
    }
}
