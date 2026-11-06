using System.ComponentModel.DataAnnotations;
//se agregaron validaciones
namespace Users.API.DTOs
{
    public record RegisterUserRequest(
        [Required(ErrorMessage = "El nombre es obligatorio.")] 
        string Nombre,

        [Required(ErrorMessage = "El apellido es obligatorio.")] 
        string Apellido,

        [Required(ErrorMessage = "El email es obligatorio.")] 
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")] 
        string Email,

        [Required(ErrorMessage = "La contraseña es obligatoria.")] 
        string Password
    );
}