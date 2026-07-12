using Microsoft.AspNetCore.Routing;

namespace Helios.Api.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
