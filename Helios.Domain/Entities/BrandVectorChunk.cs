using Pgvector;
using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class BrandVectorChunk : BaseEntity
{
    public Guid BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public string ChunkText { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public int ChunkIndex { get; set; }
    public int? TokenCount { get; set; }
}
