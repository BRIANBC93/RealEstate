# RealEstate API (.NET 8) — Prueba Técnica
**Autor: Brian Alberto Bernal Castillo — 2025**

API limpia y testeable para gestión de propiedades inmobiliarias. Construida con **.NET 8**, **C#**, **SQL Server**, **EF Core** y **NUnit**.

---

## Estructura de la solución

- **RealEstate.Domain**: Entidades de dominio (`Owner`, `Property`, `PropertyImage`, `PropertyTrace`).
- **RealEstate.Application**: DTOs, interfaces y modelos de paginación/filtrado.
- **RealEstate.Infrastructure**: `AppDbContext` (EF Core) y `PropertyService` (reglas de negocio).
- **RealEstate.Api**: Web API (controladores, JWT, Swagger).
- **RealEstate.Tests**: Pruebas unitarias (NUnit + EF Core InMemory).

---

## Requisitos previos

- .NET 8 SDK  
- SQL Server (LocalDB / Developer / Azure SQL)  
- Visual Studio 2022 o equivalente  

---

### ⚠️ Importante: Configuración de la cadena de conexión
En el archivo `appsettings.json` del proyecto **RealEstate.Api**, ajustar la sección:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=RealEstateDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

## Cómo ejecutar

- Abrir la solución **RealEstate.sln** y **levantar el proyecto `RealEstate.Api`**.  
- Al iniciar, la API expone **Swagger** en la ruta `/swagger`.  
- La base de datos se crea automáticamente si no existe (`EnsureCreated`). Para entornos reales, usar migraciones.

---

## Autenticación (JWT)

- **Endpoint**: `POST /api/auth/login`
- **Usuarios de prueba**:
  - `admin / admin123` → rol **Admin**
  - `user / user123` → rol **User**
- En Swagger, usa el botón **Authorize**, pega el token enviado por `/api/auth/login` (esquema Bearer).

---

## Endpoints principales

- `POST /api/properties`  
  Crea una propiedad. (Auth requerida)

- `PUT /api/properties/{id}`  
  Actualiza datos básicos. Usa **concurrencia optimista** con `RowVersion` (Base64). (Auth)

- `PATCH /api/properties/{id}/price`  
  Cambia el precio y guarda un registro en **PropertyTrace**. Requiere `RowVersion`. (Auth)

- `POST /api/properties/{id}/images`  
  Sube una imagen. Recibe `multipart/form-data` con campos `file` (binario) y `enabled` (booleano).  
  En el servicio se guarda el **contenido en Base64** asociado a la propiedad. (Auth)

- `GET /api/properties`  
  Lista con filtros y paginación. (Público)

- `GET /api/properties/{id}`  
  Detalle de una propiedad, incluyendo `Owner` y `RowVersion` en Base64. (Público)

- `POST /api/owners`  
  Crea un propietario. (Auth)

- `GET /api/owners/{id}`  
  Obtiene un propietario por id. (Público)

---

## Modelo de datos (resumen funcional)

- **Property**
  - `CodeInternal` **único**
  - `RowVersion` (token de concurrencia)
  - Historial de cambios de precio en **PropertyTrace**
  - Relación opcional con `Owner` (`IdOwner` puede ser nulo)

- **PropertyImage**
  - Se asocia por `IdProperty`
  - Se almacena el **contenido en Base64**
  - Campo `Enabled` para activar/desactivar

---

## Filtros y ordenamiento (listado de propiedades)

Parámetros de query soportados:

- `search` (busca en `Name`, `CodeInternal`, `Address`)
- `yearFrom`, `yearTo`
- `minPrice`, `maxPrice`
- `withImages` (true/false)
- `sortBy` (valores: `price`, `year`, `createdAt`, `name`) + `desc` (true/false)
- `page`, `pageSize`

> Nota: `sortBy` es **case-insensitive**. El default ordena por `IdProperty`.

---

## Concurrencia optimista (RowVersion)

- Las respuestas de detalle/listado incluyen `RowVersion` en **Base64**.  
- Para `PUT /properties/{id}` y `PATCH /properties/{id}/price` debes enviar el `RowVersion` recibido para evitar sobrescrituras concurrentes.  
- Si el `RowVersion` no coincide, la API responde **409 Conflict**.

---

## Subida de imágenes (Swagger)

- El `OperationFilter` ya declara el esquema `multipart/form-data` con:
  - `file` (binary) — requerido
  - `enabled` (boolean)
- Límite de tamaño por defecto: **10MB**.

---

## Buenas prácticas implementadas

- `AsNoTracking()` en lecturas.
- Validaciones por **DataAnnotations** en DTOs.
- `CodeInternal` único (índice).
- `RowVersion` como token de concurrencia.
- Paginación consistente (`PagedResult<T>`).
- Manejo de estados y excepciones coherente en controladores (404, 409, 400, etc.).

---

## Pruebas

- Proyecto: **RealEstate.Tests** (NUnit + EF Core InMemory).  
- Cobertura de casos clave:
  - Creación y recuperación de propiedades con `Owner`.  
  - Cambio de precio con registro en `PropertyTrace`.  
  - Listado con filtros y paginación.  

Ejecutar pruebas:  

```bash
dotnet test

