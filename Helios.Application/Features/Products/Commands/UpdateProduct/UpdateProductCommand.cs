using Helios.Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Helios.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<ApiResponse<Guid>>
{
    [JsonIgnore]
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SearchText { get; set; }
}
