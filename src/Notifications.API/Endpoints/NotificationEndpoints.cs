using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications");

        // POST /api/notifications/send -> 201 Created
        group.MapPost("/send", async (SendNotificationRequest request, NotificationService service) =>
        {
            var result = await service.SendAsync(request);
            return Results.Created($"/api/notifications/{result.UsuarioId}", result);
        });

        // GET /api/notifications/{userId} -> 200 OK
        group.MapGet("/{userId:guid}", async (Guid userId, NotificationService service) =>
        {
            var result = await service.GetByUserIdAsync(userId);
            return Results.Ok(result);
        });
    }
}