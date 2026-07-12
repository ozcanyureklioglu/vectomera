using Helios.Domain.Common;

namespace Helios.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
