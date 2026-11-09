namespace Orders.API.Exceptions;

public static class ErrorCodes
{
   public const string ORD_001 = "ORD-001"; // Orden no encontrada
    public const string ORD_002 = "ORD-002"; // Datos inválidos
    public const string ORD_003 = "ORD-003"; // Usuario no encontrado
    public const string ORD_004 = "ORD-004"; // Producto no encontrado
    public const string ORD_005 = "ORD-005"; // Stock insuficiente
    public const string ORD_006 = "ORD-006"; // Transición de estado inválida
    public const string ORD_007 = "ORD-007"; // Error interno
}
