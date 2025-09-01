using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace RealEstate.Api.Controllers
{
    /// <summary>
    /// Controlador encargado de manejar la autenticación de usuarios
    /// mediante la generación de tokens JWT.
    /// </summary>
    /// <remarks>
    /// Autor: Brian Alberto Bernal Castillo  
    /// Fecha de desarrollo: 31/08/2025  
    /// 
    /// Este controlador expone un endpoint de autenticación
    /// para validar credenciales de usuario y retornar un JWT
    /// con los claims correspondientes.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor que inyecta la configuración de la aplicación.
        /// </summary>
        /// <param name="config">Configuración para acceder a parámetros como la clave JWT.</param>
        public AuthController(IConfiguration config) => _config = config;

        /// <summary>
        /// Representa la solicitud de inicio de sesión con credenciales de usuario.
        /// </summary>
        /// <param name="Username">Nombre de usuario.</param>
        /// <param name="Password">Contraseña del usuario.</param>
        public record LoginRequest(string Username, string Password);

        /// <summary>
        /// Endpoint para realizar la autenticación de un usuario.
        /// Valida las credenciales y genera un JWT con los claims correspondientes.
        /// </summary>
        /// <param name="req">Credenciales enviadas en el cuerpo de la solicitud.</param>
        /// <returns>
        /// Retorna un token JWT si la autenticación es válida, 
        /// de lo contrario devuelve un 401 Unauthorized.
        /// </returns>
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest req)
        {
            // Validación de credenciales de prueba (demo users).
            // En un escenario real, aquí se debe consultar la base de datos
            // o un servicio de identidad centralizado.
            var valid = (req.Username == "admin" && req.Password == "admin123")
                        || (req.Username == "user" && req.Password == "user123");

            if (!valid)
                return Unauthorized(); // Devuelve 401 si las credenciales no son correctas.

            // Definición de los claims que se incluirán en el token.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, req.Username),
                new Claim(ClaimTypes.Role, req.Username == "admin" ? "Admin" : "User")
            };

            // Generación de la clave simétrica a partir de la configuración.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Creación del token JWT con tiempo de expiración de 8 horas.
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            // Serialización del token a string.
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Retorna el token en el cuerpo de la respuesta.
            return Ok(new { token = jwt });
        }
    }
}
