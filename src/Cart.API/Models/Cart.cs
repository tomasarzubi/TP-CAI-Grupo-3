
namespace Cart.API.Models;

public record Cart
{
    public Guid UsuarioId { get; init; }
    public List<CartItem> Items { get; init; } = [];
    public DateTime FechaActualizacion { get; init; }
}

public record CartItem
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
}

