using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidad que representa a un propietario (Owner).
/// Relaciona datos personales y las propiedades que posee.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class Owner
{
    [Key]
    public int IdOwner { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(300)]
    public string? Address { get; set; } = null!;

    public string? Photo { get; set; }

    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Propiedades asociadas a este propietario.
    /// </summary>
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}

/// <summary>
/// Entidad que representa una propiedad inmobiliaria.
/// Incluye relación con propietario, imágenes y trazabilidad.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class Property
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdProperty { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = null!;

    [Required, StringLength(300)]
    public string? Address { get; set; } = null!;

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    [Required, StringLength(64)]
    public string CodeInternal { get; set; } = null!;

    [Range(1800, 2100)]
    public int Year { get; set; }

    // Relación con propietario
    public int? IdOwner { get; set; }
    public Owner Owner { get; set; } = null!;

    // Buenas prácticas: auditoría y concurrencia
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Token de concurrencia optimista en Base64 (RowVersion).
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; } = null!;

    // Relaciones con imágenes y trazabilidad
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<PropertyTrace> Traces { get; set; } = new List<PropertyTrace>();
}

/// <summary>
/// Entidad que representa una imagen asociada a una propiedad.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PropertyImage
{
    [Key]
    public int IdPropertyImage { get; set; }

    public int IdProperty { get; set; }
    public Property Property { get; set; } = null!;

    [Required]
    public string File { get; set; } = null!;

    public bool Enabled { get; set; }

    // Buenas prácticas: control de auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Entidad que representa un rastro de auditoría o venta de una propiedad.
/// Incluye información del valor y los impuestos.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025
/// </remarks>
public class PropertyTrace
{
    [Key]
    public int IdPropertyTrace { get; set; }

    public DateTime DateSale { get; set; }
    public string Name { get; set; } = null!;
    public decimal Value { get; set; }
    public decimal Tax { get; set; }

    // Relación con propiedad
    public int IdProperty { get; set; }
    public Property Property { get; set; } = null!;

    // Buenas prácticas: auditoría
    [NotMapped]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
}
