using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Helios.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ApiResponse<List<ProductDto>>>
{
    private readonly IAppDbContext _context;

    public GetProductsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchText) || 
                p.Sku.ToLower().Contains(searchText) || 
                p.Brand.Name.ToLower().Contains(searchText)
            );
        }

        var products = await query
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Sku,
                p.Slug,
                p.Description,
                p.Brand.Name,
                p.Category.Name
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ProductDto>>.Ok(products);
    }
}
