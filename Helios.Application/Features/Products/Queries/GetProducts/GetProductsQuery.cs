using Helios.Application.Common.Models;
using MediatR;

namespace Helios.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(string? SearchText) : IRequest<ApiResponse<List<ProductDto>>>;
