namespace Notifications.API.DTOs;

public record NotificationResponse(
    Guid Id,
    Guid UsuarioId,
    string Mensaje,
    string Tipo,
    string Estado,
    DateTime Fecha
);