using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace RealEstate.Api.Controllers
{
    /// <summary>
    /// Controlador encargado de manejar la autenticaci�n de usuarios
    /// mediante la generaci�n de tokens JWT.
    /// </summary>
    /// <remarks>
    /// Autor: Brian Alberto Bernal Castillo  
    /// Fecha de desarrollo: 31/08/2025  
    /// 
    /// Este controlador expone un endpoint de autenticaci�n
    /// para validar credenciales de usuario y retornar un JWT
    /// con los claims correspondientes.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor que inyecta la configuraci�n de la aplicaci�n.
        /// </summary>
        /// <param name="config">Configuraci�n para acceder a par�metros como la clave JWT.</param>
        public AuthController(IConfiguration config) => _config = config;

        /// <summary>
        /// Representa la solicitud de inicio de sesi�n con credenciales de usuario.
        /// </summary>
        /// <param name="Username">Nombre de usuario.</param>
        /// <param name="Password">Contrase�a del usuario.</param>
        public record LoginRequest(string Username, string Password);

        /// <summary>
        /// Endpoint para realizar la autenticaci�n de un usuario.
        /// Valida las credenciales y genera un JWT con los claims correspondientes.
        /// </summary>
        /// <param name="req">Credenciales enviadas en el cuerpo de la solicitud.</param>
        /// <returns>
        /// Retorna un token JWT si la autenticaci�n es v�lida, 
        /// de lo contrario devuelve un 401 Unauthorized.
        /// </returns>
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest req)
        {
            // Validaci�n de credenciales de prueba (demo users).
            // En un escenario real, aqu� se debe consultar la base de datos
            // o un servicio de identidad centralizado.
            var valid = (req.Username == "admin" && req.Password == "admin123")
                        || (req.Username == "user" && req.Password == "user123");

            if (!valid)
                return Unauthorized(); // Devuelve 401 si las credenciales no son correctas.

            // Definici�n de los claims que se incluir�n en el token.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, req.Username),
                new Claim(ClaimTypes.Role, req.Username == "admin" ? "Admin" : "User")
            };

            // Generaci�n de la clave sim�trica a partir de la configuraci�n.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Creaci�n del token JWT con tiempo de expiraci�n de 8 horas.
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            // Serializaci�n del token a string.
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Retorna el token en el cuerpo de la respuesta.
            return Ok(new { token = jwt });
        }
    }
}
