namespace Helios.Application.Common.Events;

public class ProductEmbeddingEvent
{
    public Guid ProductId { get; set; }
    public string? Description { get; set; }
}
