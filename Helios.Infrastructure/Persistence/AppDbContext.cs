using Helios.Domain.Common;
using Helios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using Helios.Application.Common.Interfaces;

namespace Helios.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyValue> PropertyValues { get; set; }
    public DbSet<ProductProperty> ProductProperties { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<WarehouseInventory> WarehouseInventories { get; set; }
    public DbSet<BrandVectorChunk> BrandVectorChunks { get; set; }
    public DbSet<ProductVectorChunk> ProductVectorChunks { get; set; }
    public DbSet<WarehouseInventoryVectorChunk> WarehouseInventoryVectorChunks { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasPostgresExtension("vector");

        // Product - ProductProperty - PropertyValue (Many to Many through Junction)
        modelBuilder.Entity<ProductProperty>()
            .HasKey(pp => new { pp.ProductId, pp.PropertyValueId });

        modelBuilder.Entity<ProductProperty>()
            .HasOne(pp => pp.Product)
            .WithMany(p => p.ProductProperties)
            .HasForeignKey(pp => pp.ProductId);

        modelBuilder.Entity<ProductProperty>()
            .HasOne(pp => pp.PropertyValue)
            .WithMany()
            .HasForeignKey(pp => pp.PropertyValueId);
            
        // Setup MaxLengths and Indices
        modelBuilder.Entity<Brand>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => x.Sku).IsUnique();
        
        modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(255).IsRequired();
        modelBuilder.Entity<Product>().Property(p => p.Sku).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Product>().Property(p => p.Slug).HasMaxLength(255).IsRequired();

        // Warehouse Inventory Configurations
        modelBuilder.Entity<WarehouseInventory>()
            .HasIndex(wi => new { wi.WarehouseId, wi.ProductId })
            .IsUnique();

        modelBuilder.Entity<WarehouseInventory>()
            .HasOne(wi => wi.Product)
            .WithMany(p => p.WarehouseInventories)
            .HasForeignKey(wi => wi.ProductId);

        modelBuilder.Entity<WarehouseInventory>()
            .HasOne(wi => wi.Warehouse)
            .WithMany(w => w.Inventories)
            .HasForeignKey(wi => wi.WarehouseId);
            
        modelBuilder.Entity<WarehouseInventory>()
            .Property(wi => wi.Price)
            .HasColumnType("decimal(18,2)");
    }
}
