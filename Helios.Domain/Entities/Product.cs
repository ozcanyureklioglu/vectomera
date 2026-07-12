using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SearchText { get; set; }

    public Guid BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductProperty> ProductProperties { get; set; } = new List<ProductProperty>();
    public ICollection<WarehouseInventory> WarehouseInventories { get; set; } = new List<WarehouseInventory>();
    public ICollection<ProductVectorChunk> VectorChunks { get; set; } = new List<ProductVectorChunk>();
}
