namespace Helios.Domain.Entities;

public class ProductProperty
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid PropertyValueId { get; set; }
    public PropertyValue PropertyValue { get; set; } = null!;
}
