using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers
{
    /// <summary>
    /// Controlador encargado de la gestión de propietarios (Owners).
    /// Permite crear nuevos propietarios y consultar su información.
    /// </summary>
    /// <remarks>
    /// Autor: Brian Alberto Bernal Castillo  
    /// Fecha de desarrollo: 31/08/2025  
    /// 
    /// Este controlador expone endpoints seguros para la creación de propietarios 
    /// y endpoints públicos para la consulta de información de un propietario en particular.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class OwnersController : ControllerBase
    {
        private readonly IPropertyService _service;

        /// <summary>
        /// Constructor que recibe el servicio de propiedades para acceder
        /// a la lógica de negocio relacionada con propietarios.
        /// </summary>
        /// <param name="service">Servicio de propiedades inyectado vía DI.</param>
        public OwnersController(IPropertyService service) => _service = service;

        /// <summary>
        /// Crea un nuevo propietario en el sistema.
        /// </summary>
        /// <param name="dto">Objeto con la información necesaria para crear un propietario.</param>
        /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
        /// <returns>
        /// Retorna un código **201 Created** con la información del propietario recién creado,
        /// o un error de validación si los datos son incorrectos.
        /// </returns>
        [HttpPost]
        [Authorize] // Solo usuarios autenticados pueden crear propietarios.
        public async Task<IActionResult> Create([FromBody] OwnerCreateDto dto, CancellationToken ct)
        {
            // Validación del modelo recibido según las reglas de data annotations.
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // Se crea el propietario y se obtiene el nuevo Id generado.
            var id = await _service.CreateOwnerAsync(dto, ct);

            // Se consulta nuevamente el propietario recién creado para retornarlo.
            var created = await _service.GetOwnerAsync(id, ct);

            // Se retorna un 201 Created con la URL del recurso y el objeto creado.
            return CreatedAtAction(nameof(Get), new { id }, created);
        }

        /// <summary>
        /// Obtiene un propietario por su identificador único.
        /// </summary>
        /// <param name="id">Identificador del propietario a consultar.</param>
        /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
        /// <returns>
        /// Retorna un código **200 OK** con la información del propietario,
        /// o **404 Not Found** si no existe.
        /// </returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous] // Permite que cualquier usuario consulte propietarios.
        public async Task<IActionResult> Get([FromRoute] int id, CancellationToken ct)
        {
            var owner = await _service.GetOwnerAsync(id, ct);
            return owner is null ? NotFound() : Ok(owner);
        }
    }
}
