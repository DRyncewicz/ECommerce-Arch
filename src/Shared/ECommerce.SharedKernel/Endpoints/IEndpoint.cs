using Microsoft.AspNetCore.Routing;

namespace ECommerce.SharedKernel.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
