using Helios.Api.Abstractions;
using Helios.Application.Common.Interfaces;
using Helios.Application.Features.Products.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Helios.Api.Endpoints.Products;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, IProductService productService, CancellationToken cancellationToken) =>
        {
            var response = await productService.CreateProductAsync(request, cancellationToken);

            if (response.Success)
                return Results.Ok(response);

            return Results.BadRequest(response);
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .Produces<Application.Common.Models.ApiResponse<Guid>>(StatusCodes.Status200OK)
        .Produces<Application.Common.Models.ApiResponse<Guid>>(StatusCodes.Status400BadRequest);
    }
}
