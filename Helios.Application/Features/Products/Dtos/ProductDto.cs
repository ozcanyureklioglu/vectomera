namespace Helios.Application.Features.Products.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string Slug,
    string? Description,
    string BrandName,
    string CategoryName
);
