using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.DTOs;

/// <summary>
/// DTO para creaci�n de un propietario (Owner).
/// Define las validaciones necesarias al registrar un nuevo due�o.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class OwnerCreateDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = default!;

    [StringLength(300)]
    public string? Address { get; set; }

    public string? Photo { get; set; }

    public DateTime? Birthday { get; set; }
}

/// <summary>
/// DTO para consultar informaci�n b�sica de un propietario.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class OwnerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public DateTime? Birthday { get; set; }
}

/// <summary>
/// DTO para crear una nueva propiedad.
/// Incluye validaciones de datos b�sicos.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025  
/// Este objeto es utilizado en el m�todo <c>POST /api/Properties</c>.
/// </remarks>
public class PropertyCreateDto
{
    [Required, StringLength(64, MinimumLength = 3)]
    public string CodeInternal { get; set; } = default!;

    [Required, StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = default!;

    [Required, StringLength(300)]
    public string? Address { get; set; } = default!;

    [Range(1800, 2100)]
    public int Year { get; set; }

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    public int? IdOwner { get; set; }
}

/// <summary>
/// DTO para actualizaci�n de propiedades.
/// Implementa concurrencia optimista con RowVersion.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PropertyUpdateDto
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = default!;

    [Required, StringLength(300)]
    public string Address { get; set; } = default!;

    [Range(1800, 2100)]
    public int Year { get; set; }

    /// <summary>
    /// Token de concurrencia optimista en Base64.
    /// </summary>
    [Required]
    public string RowVersion { get; set; } = default!;
}

/// <summary>
/// DTO para cambio de precio de una propiedad.
/// Se registran auditor�as a trav�s de PropertyTrace.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class ChangePriceDto
{
    [Range(0, 999999999)]
    public decimal NewPrice { get; set; }

    public string? ChangedBy { get; set; }

    [Required]
    public string RowVersion { get; set; } = default!;
}

/// <summary>
/// DTO para subir im�genes asociadas a una propiedad.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PropertyImageUploadDto
{
    public byte[] Data { get; set; }
    public string ContentType { get; set; } = "application/octet-stream";
    public bool Enabled { get; set; }
}

/// <summary>
/// DTO para trazabilidad de cambios en propiedades (hist�rico de ventas o cambios de valor).
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PropertyTraceDto
{
    public int Id { get; set; }
    public DateTime DateSale { get; set; }
    public string? Name { get; set; }
    public decimal Value { get; set; }
    public decimal Tax { get; set; }
}

/// <summary>
/// Filtro para consultas de propiedades (List API).
/// Permite b�squedas avanzadas con paginaci�n y ordenamiento.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PropertyFilter
{
    public string? Search { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? WithImages { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO de salida de propiedades (consulta y detalle).
/// Incluye datos del propietario y control de concurrencia.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PropertyDto
{
    public int Id { get; set; }
    public string CodeInternal { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ImageCount { get; set; }
    public string RowVersion { get; set; } = default!;

    // Info del propietario asociado
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
}

/// <summary>
/// Modelo gen�rico de paginaci�n.
/// Utilizado en listados que devuelven colecciones.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}
