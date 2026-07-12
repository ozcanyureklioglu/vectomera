namespace Helios.Application.Features.Products.Requests;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SearchText { get; set; }
}
