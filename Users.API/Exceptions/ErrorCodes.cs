namespace Products.API.Exceptions;

public static class ErrorCodes
{
    public const string PRD_001 = "PRD-001"; // Producto no encontrado
    public const string PRD_002 = "PRD-002"; // Datos inválidos
    public const string PRD_003 = "PRD-003"; // Nombre duplicado en la categoría
    public const string PRD_004 = "PRD-004"; // Tiene órdenes activas
    public const string PRD_005 = "PRD-005"; // Error interno
}
