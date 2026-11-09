namespace Orders.API.Models;

public record OrderItem
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}

public record Order
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public List<OrderItem> Items { get; init; } = [];
    public decimal Total { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
}
