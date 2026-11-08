using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        private readonly UserRepository _repository;

        public UserService(UserRepository repository)
        {
            _repository = repository;
        }

        public async Task<User> RegisterAsync(RegisterUserRequest req)
        {
            // 1. Validacion de datos de entrada (REQ-03 -> USR-002)
            if (req == null ||
                string.IsNullOrWhiteSpace(req.Nombre) ||
                string.IsNullOrWhiteSpace(req.Apellido) ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                !req.Email.Contains("@"))
            {
                throw new BusinessRuleException("USR-002", "Los datos del usuario son inválidos.", statusCode: 400);
            }

            // 2. Verifico si el mail ya existe
            var existe = await _repository.GetByEmailAsync(req.Email);
            if (existe != null)
            {
                throw new BusinessRuleException("USR-001", $"El email '{req.Email}' ya está registrado.", statusCode: 409);
            }

            // 3. Armo el usuario nuevo
            var user = new User
            {
                Id = Guid.NewGuid(),
                Nombre = req.Nombre,
                Apellido = req.Apellido,
                Email = req.Email,
                PasswordHash = req.Password, // TODO: en la vida real esto se hashea
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IntentosFallidos = 0
            };

            await _repository.CreateAsync(user);

            // Por seguridad, el TP pide no devolver nunca el password
            user.PasswordHash = ""; 
            return user;
        }
        public async Task<User> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                throw new BusinessRuleException("USR-007", "Usuario no encontrado.", statusCode: 404); //Aunque no esta en la consgna, se agrego el error de Usuario no encontrado
            }
            return user;
        }
        public async Task<User> LoginAsync(LoginUserRequest req)
        {
            // Validacion de datos de entrada en login
            if (req == null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                throw new BusinessRuleException("USR-002", "Los datos del usuario son inválidos.", statusCode: 400);
            }

            var user = await _repository.GetByEmailAsync(req.Email);

            // 1. Usuario no existe -> 401 Unauthorized (USR-003)
            if (user == null)
            {
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.", statusCode: 401);
            }

            // 2. Cuenta bloqueada -> 403 Forbidden (USR-004 / USR-005)
            if (!user.Activo)
            {
                if (user.IntentosFallidos >= 3)
                    throw new BusinessRuleException("USR-004", "Usuario bloqueado por demasiados intentos fallidos.", statusCode: 403);
                else
                    throw new BusinessRuleException("USR-005", "Usuario bloqueado por detección de fraude.", statusCode: 403);
            }

            // 3. Contraseña incorrecta -> 401 Unauthorized (USR-003)
            if (user.PasswordHash != req.Password)
            {
                user.IntentosFallidos++;
                
                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                }
                
                await _repository.UpdateAsync(user);
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.", statusCode: 401);
            }

            user.IntentosFallidos = 0;
            await _repository.UpdateAsync(user);

            user.PasswordHash = ""; 
            return user;
        }
    }
}
