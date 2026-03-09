using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Endpoints;

namespace ECommerce.SearchService.Features.SearchProducts;

internal sealed class SearchProductsEndpoint(IQueryDispatcher dispatcher) : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search/products", async (
            string? term,
            int page,
            int pageSize,
            CancellationToken ct) =>
        {
            var query = new SearchProductsQuery(term ?? string.Empty, page, pageSize);
            var result = await dispatcher.QueryAsync(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("SearchProducts")
        .WithSummary("Full-text product search")
        .WithTags("Search")
        .Produces<SearchProductsResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
