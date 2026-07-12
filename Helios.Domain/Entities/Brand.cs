using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<BrandVectorChunk> VectorChunks { get; set; } = new List<BrandVectorChunk>();
}
