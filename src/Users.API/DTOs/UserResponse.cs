//(El campo PasswordHash nunca debe incluirse en ninguna respuesta).
public record UserResponse(Guid Id, string Nombre, string Apellido, string Email, DateTime FechaRegistro, bool Activo);