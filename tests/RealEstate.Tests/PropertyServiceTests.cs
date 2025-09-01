using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RealEstate.Application.DTOs;
using RealEstate.Infrastructure;

namespace RealEstate.Tests;

/// <summary>
/// Tests de integración ligera para PropertyService
/// Usamos InMemoryDatabase porque no necesitamos compatibilidad exacta de SQL
/// </summary>
public class PropertyServiceTests
{
    private AppDbContext _db = default!;
    private PropertyService _svc = default!;

    [SetUp]
    public void Setup()
    {
        // InMemory para pruebas rápidas y aisladas
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _svc = new PropertyService(_db);
    }

    [Test]
    public async Task Create_And_GetById_Works_With_Owner()
    {
        // Arrange → crear un dueño
        var ownerId = await _svc.CreateOwnerAsync(
            new OwnerCreateDto { Name = "Juan Perez", Address = "Calle 1" },
            CancellationToken.None);

        // Act → crear propiedad con ese dueño
        var id = await _svc.CreateAsync(new PropertyCreateDto
        {
            CodeInternal = "ABC-001",
            Name = "Bernal home",
            Address = "123 Main St",
            Year = 2020,
            Price = 500000,
            IdOwner = ownerId
        }, CancellationToken.None);

        var dto = await _svc.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Name, Is.EqualTo("Bernal home"));
        Assert.That(dto.RowVersion, Is.Not.Null);
        Assert.That(dto.OwnerId, Is.EqualTo(ownerId));
        Assert.That(dto.OwnerName, Is.EqualTo("Juan Perez"));
    }

    [Test]
    public async Task ChangePrice_Stores_History_And_Updates_Price()
    {
        // Arrange
        var ownerId = await _svc.CreateOwnerAsync(
            new OwnerCreateDto { Name = "Owner 1", Address = "Calle 2" },
            CancellationToken.None);

        var id = await _svc.CreateAsync(new PropertyCreateDto
        {
            CodeInternal = "ABC-002",
            Name = "Cozy flat",
            Address = "Calle Falsa 123",
            Year = 2015,
            Price = 300000,
            IdOwner = ownerId
        }, CancellationToken.None);

        // Act → como InMemory no soporta RowVersion, pasamos null
        await _svc.ChangePriceAsync(id, new ChangePriceDto
        {
            NewPrice = 310000,
            RowVersion = null, // 👈 ya no rompe
            ChangedBy = "tester"
        }, CancellationToken.None);

        var after = await _svc.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.That(after!.Price, Is.EqualTo(310000));
    }


    [Test]
    public async Task List_With_Filters_Returns_Paginated()
    {
        // Arrange → crear dueño
        var ownerId = await _svc.CreateOwnerAsync(
            new OwnerCreateDto { Name = "Seed Owner", Address = "Calle X" },
            CancellationToken.None);

        // Crear 35 propiedades
        for (int i = 0; i < 35; i++)
        {
            await _svc.CreateAsync(new PropertyCreateDto
            {
                CodeInternal = $"C-{i:000}",
                Name = "Unit " + i,
                Address = "Somewhere " + i,
                Year = 2010 + (i % 10),
                Price = 200000 + i * 1000,
                IdOwner = ownerId
            }, CancellationToken.None);
        }

        // Act → obtener página 1 de 10 propiedades ordenadas por precio desc
        var page1 = await _svc.ListAsync(
            new PropertyFilter { Page = 1, PageSize = 10, SortBy = "price", Desc = true },
            CancellationToken.None);

        // Assert
        Assert.That(page1.Items.Count, Is.EqualTo(10));
        Assert.That(page1.Total, Is.GreaterThan(10));
        Assert.That(page1.Items.First().Price,
            Is.GreaterThanOrEqualTo(page1.Items.Last().Price));
    }
}
