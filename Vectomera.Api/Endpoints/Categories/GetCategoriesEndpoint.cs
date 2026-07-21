using Vectomera.Api.Abstractions;
using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Vectomera.Api.Endpoints.Categories;

public record CategoryLookupDto(Guid Id, string Name);

public class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (IAppDbContext context, CancellationToken cancellationToken) =>
        {
            var categories = await context.Categories
                .AsNoTracking()
                .Select(c => new CategoryLookupDto(c.Id, c.Name))
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<List<CategoryLookupDto>>.Ok(categories));
        })
        .WithName("GetCategories")
        .WithTags("Categories")
        .Produces<ApiResponse<List<CategoryLookupDto>>>(StatusCodes.Status200OK);
    }
}
