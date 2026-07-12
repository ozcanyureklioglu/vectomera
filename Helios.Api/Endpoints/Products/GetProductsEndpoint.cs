using Helios.Api.Abstractions;
using Helios.Application.Features.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Helios.Api.Endpoints.Products;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async ([FromQuery] string? searchText, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery(searchText);
            var response = await mediator.Send(query, cancellationToken);
            
            return Results.Ok(response);
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<Application.Common.Models.ApiResponse<List<ProductDto>>>(StatusCodes.Status200OK);
    }
}
