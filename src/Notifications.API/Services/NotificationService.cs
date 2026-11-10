using Notifications.API.DTOs;
using Notifications.API.Exceptions;

namespace Notifications.API.Services;

public class NotificationService(UsersClient users)
{
    private static readonly List<NotificationResponse> _notificationsDb = new();

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest req)
    {
        // NTF-002 (400): campos faltantes o tipo no reconocido
        if (req == null || req.UsuarioId == Guid.Empty ||
            string.IsNullOrWhiteSpace(req.Mensaje) ||
            (req.Tipo != "Email" && req.Tipo != "Push" && req.Tipo != "SMS"))
        {
            throw new ValidationException(ErrorCodes.NTF_002,
                "Los datos de la notificación son inválidos.");
        }

        // NTF-001 (404): el usuario no existe en Users.API
        if (!await users.ExistsAsync(req.UsuarioId))
            throw new NotFoundException(ErrorCodes.NTF_001,
                "El usuario destinatario no fue encontrado.");

        var notification = new NotificationResponse(
            Guid.NewGuid(),
            req.UsuarioId,
            req.Mensaje,
            req.Tipo,
            "Enviada",
            DateTime.UtcNow
        );

        _notificationsDb.Add(notification);
        return notification;
    }

    public Task<List<NotificationResponse>> GetByUserIdAsync(Guid userId)
    {
        var resultado = new List<NotificationResponse>();

        foreach (var n in _notificationsDb)
        {
            if (n.UsuarioId == userId)
                resultado.Add(n);
        }

        // NTF-003 (404): el usuario no tiene notificaciones
        if (resultado.Count == 0)
            throw new NotFoundException(ErrorCodes.NTF_003,
                "No se encontraron notificaciones para el usuario.");

        return Task.FromResult(resultado);
    }
}