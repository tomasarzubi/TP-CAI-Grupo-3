namespace Cart.API.DTOs;

public record AddCartItemRequest
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
}