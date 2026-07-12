namespace Helios.Application.Features.Products.Queries.GetProducts;

public record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string Slug,
    string? Description,
    string BrandName,
    string CategoryName
);
