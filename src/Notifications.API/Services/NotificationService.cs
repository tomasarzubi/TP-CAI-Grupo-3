using Notifications.API.DTOs;
using Notifications.API.Exceptions;

namespace Notifications.API.Services;

public class NotificationService
{
    private static readonly List<NotificationResponse> _notificationsDb = new();

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest req)
    {
        // Validacion REQ-03 -> NTF-002 (400 Bad Request)
        if (req == null || string.IsNullOrWhiteSpace(req.Mensaje) ||
            (req.Tipo != "Email" && req.Tipo != "Push" && req.Tipo != "SMS"))
        {
            throw new BusinessRuleException("NTF-002", "Los datos de la notificación son inválidos.", statusCode: 400);
        }

        // Validacion de Usuario -> NTF-001 (404 Not Found)
        if (req.UsuarioId == Guid.Empty)
        {
            throw new BusinessRuleException("NTF-001", "El usuario destinatario no fue encontrado.", statusCode: 404);
        }

        var notification = new NotificationResponse(
            Guid.NewGuid(),
            req.UsuarioId,
            req.Mensaje,
            req.Tipo,
            "Enviada",
            DateTime.UtcNow
        );

        _notificationsDb.Add(notification);
        return await Task.FromResult(notification);
    }

    public async Task<List<NotificationResponse>> GetByUserIdAsync(Guid userId)
    {
        var userNotifications = _notificationsDb.Where(n => n.UsuarioId == userId).ToList();

        if (!userNotifications.Any())
        {
            throw new BusinessRuleException("NTF-003", "No se encontraron notificaciones para el usuario.", statusCode: 404);
        }

        return await Task.FromResult(userNotifications);
    }
}