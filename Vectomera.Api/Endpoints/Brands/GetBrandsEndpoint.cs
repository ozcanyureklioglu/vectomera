using Vectomera.Api.Abstractions;
using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Vectomera.Api.Endpoints.Brands;

public record BrandLookupDto(Guid Id, string Name);

public class GetBrandsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/brands", async (IAppDbContext context, CancellationToken cancellationToken) =>
        {
            var brands = await context.Brands
                .AsNoTracking()
                .Select(b => new BrandLookupDto(b.Id, b.Name))
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<List<BrandLookupDto>>.Ok(brands));
        })
        .WithName("GetBrands")
        .WithTags("Brands")
        .Produces<ApiResponse<List<BrandLookupDto>>>(StatusCodes.Status200OK);
    }
}
