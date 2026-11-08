namespace Users.API.Exceptions;

public static class ErrorCodes
{
    public const string USR_001 = "USR-001"; // El email ya está registrado
    public const string USR_002 = "USR-002"; // Los datos del usuario son inválidos
    public const string USR_003 = "USR-003"; // Credenciales incorrectas
    public const string USR_004 = "USR-004"; // Usuario bloqueado por demasiados intentos fallidos
    public const string USR_005 = "USR-005"; // Usuario bloqueado por detección de fraude
    public const string USR_006 = "USR-006"; // Error interno al procesar el usuario
    public const string USR_007 = "USR-007"; // Aunque no esta en la consgna, se agrego el error de Usuario no encontrado
}
