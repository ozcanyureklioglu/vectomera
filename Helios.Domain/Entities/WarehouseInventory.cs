using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class WarehouseInventory : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int AvailableStock { get; set; }
    public int IncomingStock { get; set; }
    public int OutgoingStock { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }

    public ICollection<WarehouseInventoryVectorChunk> VectorChunks { get; set; } = new List<WarehouseInventoryVectorChunk>();
}
