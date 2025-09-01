using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure;

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _db;
    public PropertyService(AppDbContext db) => _db = db;

    // OWNER
    public async Task<int> CreateOwnerAsync(OwnerCreateDto dto, CancellationToken ct)
    {
        var owner = new Owner
        {
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            Photo = dto.Photo,
            Birthday = dto.Birthday
        };
        _db.Owners.Add(owner);
        await _db.SaveChangesAsync(ct);
        return owner.IdOwner;
    }

    public async Task<OwnerDto?> GetOwnerAsync(int id, CancellationToken ct)
    {
        var o = await _db.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.IdOwner == id, ct);
        if (o == null) return null;
        return new OwnerDto
        {
            Id = o.IdOwner,
            Name = o.Name,
            Address = o.Address,
            Birthday = o.Birthday
        };
    }

    // PROPERTY
    public async Task<int> CreateAsync(PropertyCreateDto dto, CancellationToken ct)
    {
        if (dto.Year < 1800 || dto.Year > DateTime.UtcNow.Year + 1)
            throw new ArgumentOutOfRangeException(nameof(dto.Year));
        if (await _db.Properties.AnyAsync(p => p.CodeInternal == dto.CodeInternal, ct))
            throw new InvalidOperationException("CodeInternal must be unique.");

        // If IdOwner provided, ensure exists
        if (dto.IdOwner.HasValue)
        {
            var exists = await _db.Owners.AnyAsync(o => o.IdOwner == dto.IdOwner.Value, ct);
            if (!exists) throw new KeyNotFoundException("Owner not found.");
        }

        var entity = new Property
        {
            CodeInternal = dto.CodeInternal.Trim(),
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            Year = dto.Year,
            Price = dto.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IdOwner = dto.IdOwner
        };

        _db.Properties.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.IdProperty;
    }

    public async Task UpdateAsync(int id, PropertyUpdateDto dto, CancellationToken ct)
    {
        var entity = await _db.Properties.FirstOrDefaultAsync(p => p.IdProperty == id, ct)
            ?? throw new KeyNotFoundException("Property not found.");

        var rowVersion = Convert.FromBase64String(dto.RowVersion);
        _db.Entry(entity).Property(p => p.RowVersion).OriginalValue = rowVersion;

        entity.Name = dto.Name.Trim();
        entity.Address = dto.Address.Trim();
        entity.Year = dto.Year;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task ChangePriceAsync(int id, ChangePriceDto dto, CancellationToken ct)
    {
        var entity = await _db.Properties.FirstOrDefaultAsync(p => p.IdProperty == id, ct)
            ?? throw new KeyNotFoundException("Property not found.");

        // Solo aplicar concurrencia si RowVersion fue provista
        if (!string.IsNullOrEmpty(dto.RowVersion))
        {
            //var rowVersion = HexStringToByteArray(dto.RowVersion);
            var rowVersion = Convert.FromBase64String(dto.RowVersion);
            _db.Entry(entity).Property(p => p.RowVersion).OriginalValue = rowVersion;
        }

        if (dto.NewPrice == entity.Price) return;

        var trace = new PropertyTrace
        {
            IdProperty = entity.IdProperty,
            DateSale = DateTime.UtcNow,
            Name = dto.ChangedBy ?? "Price change",
            Value = dto.NewPrice,
            Tax = 0m
        };

        entity.Price = dto.NewPrice;
        entity.UpdatedAt = DateTime.UtcNow;

        _db.PropertyTraces.Add(trace);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddImageAsync(int id, PropertyImageUploadDto dto, CancellationToken ct)
    {
        if (dto.Data == null || dto.Data.Length == 0) throw new InvalidOperationException("Empty image data.");

        var entity = await _db.Properties.Include(p => p.Images).FirstOrDefaultAsync(p => p.IdProperty == id, ct)
            ?? throw new KeyNotFoundException("Property not found.");

        var img = new PropertyImage
        {
            IdProperty = id,
            File = Convert.ToBase64String(dto.Data),
            Enabled = dto.Enabled,
            CreatedAt = DateTime.UtcNow
        };

        _db.PropertyImages.Add(img);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PropertyDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _db.Properties
            .AsNoTracking()
            .Where(p => p.IdProperty == id)
            .Select(p => new PropertyDto
            {
                Id = p.IdProperty,
                CodeInternal = p.CodeInternal,
                Name = p.Name,
                Address = p.Address,
                Year = p.Year,
                Price = p.Price,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ImageCount = p.Images.Count,
                RowVersion = p.RowVersion != null ? Convert.ToBase64String(p.RowVersion) : string.Empty,
                OwnerId = p.IdOwner,
                OwnerName = p.Owner != null ? p.Owner.Name : null
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<PropertyDto>> ListAsync(PropertyFilter filter, CancellationToken ct)
    {
        var query = _db.Properties.AsNoTracking().Include(p => p.Images).Include(p => p.Owner).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(p =>
                p.Name.Contains(s) ||
                p.CodeInternal.Contains(s) ||
                p.Address.Contains(s));
        }
        if (filter.YearFrom.HasValue)
            query = query.Where(p => p.Year >= filter.YearFrom.Value);
        if (filter.YearTo.HasValue)
            query = query.Where(p => p.Year <= filter.YearTo.Value);
        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        if (filter.WithImages.HasValue)
            query = query.Where(p => (p.Images.Count > 0) == filter.WithImages.Value);

        // Sorting
        var desc = filter.Desc;
        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "price" => (desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price)),
            "year" => (desc ? query.OrderByDescending(p => p.Year) : query.OrderBy(p => p.Year)),
            "createdat" => (desc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)),
            "name" => (desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)),
            _ => query.OrderBy(p => p.IdProperty)
        };

        var total = await query.CountAsync(ct);
        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 200);
        var items = await query.Skip((page - 1) * size).Take(size)
            .Select(p => new PropertyDto
            {
                Id = p.IdProperty,
                CodeInternal = p.CodeInternal,
                Name = p.Name,
                Address = p.Address,
                Year = p.Year,
                Price = p.Price,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ImageCount = p.Images.Count,
                RowVersion = p.RowVersion != null ? Convert.ToBase64String(p.RowVersion) : string.Empty,
                OwnerId = p.IdOwner,
                OwnerName = p.Owner != null ? p.Owner.Name : null
            })
            .ToListAsync(ct);

        return new PagedResult<PropertyDto>
        {
            Page = page,
            PageSize = size,
            Total = total,
            Items = items
        };
    }

    // Método auxiliar
    private static byte[] HexStringToByteArray(string hex)
    {
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex.Substring(2);

        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

}
