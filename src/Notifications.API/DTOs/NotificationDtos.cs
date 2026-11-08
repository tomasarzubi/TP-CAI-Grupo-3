namespace Notifications.API.DTOs;

public record SendNotificationRequest(
    Guid UsuarioId,
    string Mensaje,
    string Tipo // Email | Push | SMS
);

public record NotificationResponse(
    Guid Id,
    Guid UsuarioId,
    string Mensaje,
    string Tipo,
    string Estado,
    DateTime FechaEnvio
);