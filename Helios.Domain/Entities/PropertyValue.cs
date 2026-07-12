using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class PropertyValue : BaseEntity
{
    public string Value { get; set; } = string.Empty;

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;
}
