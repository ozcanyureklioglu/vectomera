using Vectomera.Api.Abstractions;
using Vectomera.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Vectomera.Api.Endpoints.Products;

public class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id:guid}", async (Guid id, IProductService productService, CancellationToken cancellationToken) =>
        {
            var response = await productService.DeleteProductAsync(id, cancellationToken);

            if (response.Success)
                return Results.Ok(response);

            return Results.BadRequest(response);
        })
        .WithName("DeleteProduct")
        .WithTags("Products")
        .Produces<Application.Common.Models.ApiResponse<bool>>(StatusCodes.Status200OK)
        .Produces<Application.Common.Models.ApiResponse<bool>>(StatusCodes.Status400BadRequest);
    }
}
