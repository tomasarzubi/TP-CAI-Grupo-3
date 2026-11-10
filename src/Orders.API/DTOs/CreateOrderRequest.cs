using System.ComponentModel.DataAnnotations;
namespace Orders.API.DTOs;
public record OrderItemRequest
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
}

public record CreateOrderRequest
{
    public Guid UsuarioId { get; init; }

    [Required(ErrorMessage = "La lista de items es requerida.")]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un item.")]
    public List<OrderItemRequest> Items { get; init; } = [];
}
