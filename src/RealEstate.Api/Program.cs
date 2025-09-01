using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure;

/// <summary>
/// Punto de entrada principal de la aplicación RealEstate API.
/// Configura los servicios, autenticación, base de datos, Swagger y middleware.
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025  
/// 
/// Este archivo sigue las buenas prácticas de ASP.NET Core, manteniendo el arranque limpio:
/// - Inyección de dependencias (DbContext, servicios de negocio).
/// - Configuración de JWT para seguridad.
/// - Documentación y soporte de carga de archivos en Swagger.
/// - Migración automática de la base de datos si no existe.
/// </remarks>
var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// 1. Configuración de servicios
// ---------------------------

// Controladores MVC
builder.Services.AddControllers();

// DbContext (SQL Server con cadena de conexión desde appsettings.json)
var connString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=RealEstateDb;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connString));

// Servicios de negocio (PropertyService)
builder.Services.AddScoped<IPropertyService, PropertyService>();

// ---------------------------
// 2. Configuración de Autenticación con JWT
// ---------------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ThisIsASuperStrongSecretKeyBRIANBERNAL2025";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateLifetime = true
    };
});

// ---------------------------
// 3. Swagger (OpenAPI)
// ---------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealEstate API", Version = "v1" });

    // Configuración de autenticación JWT en Swagger
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    // Soporte de carga de archivos (multipart/form-data)
    c.OperationFilter<FileUploadOperationFilter>();

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ---------------------------
// 4. Configuración del pipeline HTTP
// ---------------------------

// Aplica migraciones automáticas: crea la BD si no existe.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Swagger UI en la raíz (http://localhost:<puerto>/)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RealEstate API V1");
    c.RoutePrefix = string.Empty;
});

// Redirección hacia Swagger por defecto
app.MapGet("/", () => Results.Redirect("/swagger"));

// Middleware de seguridad
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Mapea todos los controladores
app.MapControllers();

// Ejecuta la aplicación
app.Run();

/// <summary>
/// Filtro para permitir la carga de archivos en Swagger UI.
/// Define los parámetros esperados en la request (file y enabled).
/// </summary>
/// <remarks>
/// Autor: Brian Alberto Bernal Castillo  
/// Fecha de desarrollo: 31/08/2025  
/// 
/// Este filtro personaliza la documentación de Swagger para soportar
/// la carga de archivos con `multipart/form-data`.
/// </remarks>
public class FileUploadOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile));

        if (fileParams.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema { Type = "string", Format = "binary" },
                                ["enabled"] = new OpenApiSchema { Type = "boolean" }
                            },
                            Required = new HashSet<string> { "file" }
                        }
                    }
                }
            };
        }
    }
}
