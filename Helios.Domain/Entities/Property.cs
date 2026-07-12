using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class Property : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<PropertyValue> Values { get; set; } = new List<PropertyValue>();
}
