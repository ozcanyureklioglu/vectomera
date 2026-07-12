using Helios.Api.Abstractions;
using Helios.Application.Features.Products.Commands.CreateProduct;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Helios.Api.Endpoints.Products;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(command, cancellationToken);
            
            if (response.Success)
            {
                return Results.Ok(response);
            }
            
            return Results.BadRequest(response);
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .Produces<Application.Common.Models.ApiResponse<Guid>>(StatusCodes.Status200OK)
        .Produces<Application.Common.Models.ApiResponse<Guid>>(StatusCodes.Status400BadRequest);
    }
}
