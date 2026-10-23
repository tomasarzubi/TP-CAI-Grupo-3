
namespace Cart.API.Exceptions;

public static class ErrorCodes
{
    public const string CRT_001 = "CRT-001"; // Carrito no encontrado
    public const string CRT_002 = "CRT-002"; // Producto no encontrado
    public const string CRT_003 = "CRT-003"; // Stock insuficiente
    public const string CRT_004 = "CRT-004"; // Cantidad inválida
    public const string CRT_005 = "CRT-005"; // Error interno del carrito
}
