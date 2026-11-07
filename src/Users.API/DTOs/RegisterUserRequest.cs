using System.ComponentModel.DataAnnotations;
//se agregaron validaciones
namespace Users.API.DTOs;

public record RegisterUserRequest(
    [Required(ErrorMessage = "El nombre es requerido.")] 
    string Nombre,

    [Required(ErrorMessage = "El apellido es requerido.")] 
    string Apellido,

    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato de email es inválido.")] 
    string Email,

    [Required(ErrorMessage = "La contraseña es requerida.")] 
    string Password
);