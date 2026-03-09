using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.OrderService.Features.PlaceOrder;

internal sealed class PlaceOrderEndpoint(ICommandDispatcher dispatcher) : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async ([FromBody] PlaceOrderCommand command, CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/orders/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithName("PlaceOrder")
        .WithSummary("Place a new order")
        .WithTags("Orders")
        .Produces<object>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
