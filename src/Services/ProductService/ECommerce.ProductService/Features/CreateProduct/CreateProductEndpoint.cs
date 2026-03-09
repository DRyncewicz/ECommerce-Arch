using ECommerce.ProductService.Domain;
using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.ProductService.Features.CreateProduct;

internal sealed class CreateProductEndpoint(ICommandDispatcher dispatcher) : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async ([FromBody] CreateProductRequest request, CancellationToken ct) =>
        {
            var attributes = request.Attributes?
                .Select(a => new ProductAttribute(a.Key, a.Value, a.Unit))
                .ToList() ?? [];

            var command = new CreateProductCommand(
                request.Name,
                request.Description,
                request.BasePrice,
                request.CategoryId,
                attributes);

            var result = await dispatcher.SendAsync(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/products/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithTags("Products")
        .Produces<object>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
