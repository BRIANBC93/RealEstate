using Microsoft.EntityFrameworkCore;

namespace RealEstate.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<PropertyTrace> PropertyTraces => Set<PropertyTrace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Owner -> table Owner, IdOwner
        modelBuilder.Entity<Owner>(b =>
        {
            b.ToTable("Owner");
            b.HasKey(o => o.IdOwner);
            b.Property(o => o.IdOwner).HasColumnName("IdOwner");
            b.Property(o => o.Name).HasMaxLength(200).IsRequired();
            b.Property(o => o.Address).HasMaxLength(300);
            b.Property(o => o.Photo);
            b.Property(o => o.Birthday);
            b.HasMany(o => o.Properties).WithOne(p => p.Owner).HasForeignKey(p => p.IdOwner);
        });

        modelBuilder.Entity<Property>(b =>
        {
            b.ToTable("Property");
            b.HasKey(p => p.IdProperty);
            b.Property(p => p.IdProperty).HasColumnName("IdProperty");
            b.Property(p => p.CodeInternal).HasMaxLength(64).IsRequired();
            b.HasIndex(p => p.CodeInternal).IsUnique();
            b.Property(p => p.Name).HasMaxLength(200).IsRequired();
            b.Property(p => p.Address).HasMaxLength(300).IsRequired();
            b.Property(p => p.Year).IsRequired();
            b.Property(p => p.Price).HasColumnType("decimal(18,2)");
            b.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(p => p.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(p => p.RowVersion).IsRowVersion();
            b.Property(p => p.IdOwner).HasColumnName("IdOwner");
            b.HasMany(p => p.Images).WithOne(i => i.Property!).HasForeignKey(i => i.IdProperty).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(p => p.Traces).WithOne(t => t.Property!).HasForeignKey(t => t.IdProperty).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyImage>(b =>
        {
            b.ToTable("PropertyImage");
            b.HasKey(i => i.IdPropertyImage);
            b.Property(i => i.IdPropertyImage).HasColumnName("IdPropertyImage");
            b.Property(i => i.IdPropertyImage).IsRequired();
            b.Property(i => i.File).HasColumnName("file").HasColumnType("varbinary(max)").IsRequired();
            b.Property(i => i.Enabled).HasDefaultValue(true);
            b.Property(i => i.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<PropertyTrace>(b =>
        {
            b.ToTable("PropertyTrace");
            b.HasKey(t => t.IdPropertyTrace);
            b.Property(t => t.IdPropertyTrace).HasColumnName("IdPropertyTrace");
            b.Property(t => t.IdPropertyTrace).IsRequired();
            b.Property(t => t.DateSale).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(t => t.Name).HasMaxLength(200);
            b.Property(t => t.Value).HasColumnType("decimal(18,2)");
            b.Property(t => t.Tax).HasColumnType("decimal(18,2)");
        });
    }
}
