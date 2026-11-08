using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

public record UpdateCartItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; init; }
}