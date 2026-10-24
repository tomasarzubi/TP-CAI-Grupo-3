namespace Cart.API.DTOs;

public record UpdateCartItemRequest
{
    public int Cantidad { get; init; }
}