using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public record UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "El estado es requerido.")]
    public string Estado { get; init; } = string.Empty;
}
public record UpdateOrderStatusResponse(Guid Id, string Estado, DateTime FechaActualizacion);