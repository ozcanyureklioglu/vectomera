using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }

    public ICollection<WarehouseInventory> Inventories { get; set; } = new List<WarehouseInventory>();
}
