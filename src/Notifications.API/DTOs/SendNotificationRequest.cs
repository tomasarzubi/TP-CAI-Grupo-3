namespace Notifications.API.DTOs;

public record SendNotificationRequest(
    Guid UsuarioId,
    string Mensaje,
    string Tipo
);