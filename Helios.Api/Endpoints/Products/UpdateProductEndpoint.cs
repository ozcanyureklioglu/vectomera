using Helios.Api.Abstractions;
using Helios.Application.Common.Interfaces;
using Helios.Application.Features.Products.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Helios.Api.Endpoints.Products;

public class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:guid}", async (Guid id, [FromBody] UpdateProductRequest request, IProductService productService, CancellationToken cancellationToken) =>
        {
            var response = await productService.UpdateProductAsync(id, request, cancellationToken);

            if (response.Success)
                return Results.Ok(response);

            return Results.BadRequest(response);
        })
        .WithName("UpdateProduct")
        .WithTags("Products")
        .Produces<Application.Common.Models.ApiResponse<Guid>>(StatusCodes.Status200OK)
        .Produces<Application.Common.Models.ApiResponse<Guid>>(StatusCodes.Status400BadRequest);
    }
}
