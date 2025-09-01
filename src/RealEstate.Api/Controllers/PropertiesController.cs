using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers
{
    /// <summary>
    /// Controlador encargado de la gestión de propiedades (Properties).
    /// Permite listar, consultar, crear, actualizar y administrar cambios de precio e imágenes de propiedades.
    /// </summary>
    /// <remarks>
    /// Autor: Brian Alberto Bernal Castillo  
    /// Fecha de desarrollo: 31/08/2025  
    /// 
    /// Este controlador maneja las operaciones principales relacionadas con propiedades,
    /// asegurando buenas prácticas como validación de modelos, manejo de concurrencia
    /// y control de acceso mediante JWT.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyService _service;

        /// <summary>
        /// Constructor que recibe el servicio de propiedades para acceder a la lógica de negocio.
        /// </summary>
        /// <param name="service">Servicio de propiedades inyectado vía DI.</param>
        public PropertiesController(IPropertyService service) => _service = service;

        /// <summary>
        /// Lista todas las propiedades con filtros y paginación.
        /// </summary>
        /// <param name="filter">Objeto con criterios de filtrado (año, precio, imágenes, etc.).</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>Un listado paginado de propiedades.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> List([FromQuery] PropertyFilter filter, CancellationToken ct)
            => Ok(await _service.ListAsync(filter, ct));

        /// <summary>
        /// Obtiene el detalle de una propiedad por su Id.
        /// </summary>
        /// <param name="id">Identificador de la propiedad.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>La información detallada de la propiedad, o 404 si no existe.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _service.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>
        /// Crea una nueva propiedad en el sistema.
        /// </summary>
        /// <param name="dto">Datos de la propiedad a registrar.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>Un código 201 con la propiedad creada.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] PropertyCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var id = await _service.CreateAsync(dto, ct);
            var created = await _service.GetByIdAsync(id, ct);

            return CreatedAtAction(nameof(GetById), new { id }, created);
        }

        /// <summary>
        /// Actualiza la información de una propiedad existente.
        /// </summary>
        /// <param name="id">Id de la propiedad.</param>
        /// <param name="dto">Datos actualizados de la propiedad.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>
        /// Retorna **204 NoContent** si se actualiza correctamente,  
        /// **404 NotFound** si no existe,  
        /// o **409 Conflict** si ocurre un conflicto de concurrencia.
        /// </returns>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PropertyUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                await _service.UpdateAsync(id, dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The property was modified by another process. Please refresh and retry.");
            }
        }

        /// <summary>
        /// Cambia el precio de una propiedad y genera un rastro de auditoría.
        /// </summary>
        /// <param name="id">Id de la propiedad.</param>
        /// <param name="dto">Objeto con el nuevo precio y control de concurrencia.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>NoContent si se actualiza correctamente, o códigos de error según corresponda.</returns>
        [HttpPatch("{id:int}/price")]
        [Authorize]
        public async Task<IActionResult> ChangePrice([FromRoute] int id, [FromBody] ChangePriceDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                await _service.ChangePriceAsync(id, dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The property was modified by another process. Please refresh and retry.");
            }
        }

        /// <summary>
        /// Sube una imagen asociada a una propiedad.
        /// </summary>
        /// <param name="id">Id de la propiedad.</param>
        /// <param name="file">Archivo de imagen (hasta 10MB).</param>
        /// <param name="enabled">Indica si la imagen debe estar activa.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>Un código 204 si la imagen se carga correctamente.</returns>
        [HttpPost("{id:int}/images")]
        //[Authorize] // Habilitar si solo usuarios autenticados pueden subir imágenes.
        [RequestSizeLimit(10_000_000)] // Límite de 10MB
        public async Task<IActionResult> UploadImage(
            [FromRoute] int id,
            [FromForm] IFormFile file,
            [FromForm] bool enabled,
            CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Empty file");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);

            var dto = new PropertyImageUploadDto
            {
                Data = ms.ToArray(),
                ContentType = file.ContentType ?? "application/octet-stream",
                Enabled = enabled
            };

            await _service.AddImageAsync(id, dto, ct);
            return NoContent();
        }
    }
}
