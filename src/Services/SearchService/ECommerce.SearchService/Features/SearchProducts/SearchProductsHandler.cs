using ECommerce.SearchService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Results;

namespace ECommerce.SearchService.Features.SearchProducts;

internal sealed class SearchProductsHandler(IProductSearchRepository repository)
    : IQueryHandler<SearchProductsQuery, SearchProductsResult>
{
    public async Task<Result<SearchProductsResult>> HandleAsync(
        SearchProductsQuery query, CancellationToken ct = default)
    {
        var (hits, total) = await repository.SearchAsync(query.Term, query.Page, query.PageSize, ct);

        var items = hits.Select(doc => new ProductSearchItem(
            doc.Id,
            doc.Name,
            doc.Description,
            doc.CategoryId,
            doc.BasePrice,
            doc.Attributes.Select(a => new AttributeItem(a.Key, a.Value, a.Unit)).ToList()
        )).ToList();

        return Result.Success(new SearchProductsResult(items, total, query.Page, query.PageSize));
    }
}
