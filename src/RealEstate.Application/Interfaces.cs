using RealEstate.Application.DTOs;

namespace RealEstate.Application.Interfaces;

/// <summary>
/// Contrato para los servicios de negocio relacionados con Propiedades y Propietarios.
/// Define las operaciones CRUD y funcionalidades adicionales como carga de imágenes y cambio de precio.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public interface IPropertyService
{
    // ----------------------------
    // Operaciones sobre Propietarios (Owners)
    // ----------------------------

    /// <summary>
    /// Crea un nuevo propietario en el sistema.
    /// </summary>
    /// <param name="dto">Datos para la creación del propietario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El identificador único (Id) del propietario creado.</returns>
    Task<int> CreateOwnerAsync(OwnerCreateDto dto, CancellationToken ct);

    /// <summary>
    /// Obtiene los datos de un propietario por su Id.
    /// </summary>
    /// <param name="id">Identificador del propietario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Un <see cref="OwnerDto"/> con los datos del propietario, o null si no existe.</returns>
    Task<OwnerDto?> GetOwnerAsync(int id, CancellationToken ct);

    // ----------------------------
    // Operaciones sobre Propiedades (Properties)
    // ----------------------------

    /// <summary>
    /// Crea una nueva propiedad en el sistema.
    /// </summary>
    /// <param name="dto">Datos básicos de la propiedad a registrar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El Id de la nueva propiedad.</returns>
    Task<int> CreateAsync(PropertyCreateDto dto, CancellationToken ct);

    /// <summary>
    /// Actualiza los datos de una propiedad existente.
    /// </summary>
    /// <param name="id">Id de la propiedad a actualizar.</param>
    /// <param name="dto">Datos de la propiedad con concurrencia optimista.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateAsync(int id, PropertyUpdateDto dto, CancellationToken ct);

    /// <summary>
    /// Cambia el precio de una propiedad y registra trazabilidad.
    /// </summary>
    /// <param name="id">Id de la propiedad.</param>
    /// <param name="dto">Objeto con el nuevo precio y datos de auditoría.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task ChangePriceAsync(int id, ChangePriceDto dto, CancellationToken ct);

    /// <summary>
    /// Asocia una imagen a una propiedad.
    /// </summary>
    /// <param name="id">Id de la propiedad.</param>
    /// <param name="dto">Imagen y metadatos asociados.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddImageAsync(int id, PropertyImageUploadDto dto, CancellationToken ct);

    /// <summary>
    /// Obtiene una propiedad por su Id.
    /// </summary>
    /// <param name="id">Id de la propiedad.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Un <see cref="PropertyDto"/> con la información, o null si no existe.</returns>
    Task<PropertyDto?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Lista propiedades aplicando filtros, paginación y ordenamiento.
    /// </summary>
    /// <param name="filter">Criterios de búsqueda y paginación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Un resultado paginado con las propiedades encontradas.</returns>
    Task<PagedResult<PropertyDto>> ListAsync(PropertyFilter filter, CancellationToken ct);
}
