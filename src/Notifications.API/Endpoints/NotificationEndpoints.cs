
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Endpoints
{
    public static class NotificationEndpoints
    {
        public static void MapNotificationEndpoints(this WebApplication app)
        {
            // POST: enviar notificación
            app.MapPost("/api/notifications/send", async (SendNotificationRequest req, NotificationService service) =>
            {
                var notification = await service.SendAsync(req);
                return Results.Created($"/api/notifications/{notification.UsuarioId}", notification);
            })
            .WithTags("Notifications")
            .WithSummary("Registrar y simular envío de notificación")
            .Produces<NotificationResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500);

            // GET: obtener notificaciones por usuario
            app.MapGet("/api/notifications/{userId:guid}", async (Guid userId, NotificationService service) =>
            {
                var notifications = await service.GetByUserIdAsync(userId);
                return Results.Ok(notifications);
            })
            .WithTags("Notifications")
            .WithSummary("Listar notificaciones de un usuario")
            .Produces<List<NotificationResponse>>(200)
            .ProducesProblem(404)
            .ProducesProblem(500);
        }
    }
}
