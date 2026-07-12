using Helios.Application.Common.Models;
using MediatR;

namespace Helios.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<ApiResponse<Guid>>
{
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SearchText { get; set; }

    public List<Guid>? PropertyValueIds { get; set; }
}
