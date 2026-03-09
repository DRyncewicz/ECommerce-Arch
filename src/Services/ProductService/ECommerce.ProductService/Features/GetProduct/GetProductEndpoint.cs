using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Endpoints;

namespace ECommerce.ProductService.Features.GetProduct;

internal sealed class GetProductEndpoint(IQueryDispatcher dispatcher) : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (Guid id, CancellationToken ct) =>
        {
            var result = await dispatcher.QueryAsync(new GetProductQuery(id), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetProduct")
        .WithSummary("Get a product by ID")
        .WithTags("Products")
        .Produces<ProductResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
