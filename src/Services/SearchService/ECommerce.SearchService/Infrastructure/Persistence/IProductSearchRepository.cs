using ECommerce.SearchService.Models;

namespace ECommerce.SearchService.Infrastructure.Persistence;

internal interface IProductSearchRepository
{
    Task IndexAsync(ProductDocument document, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductDocument> Hits, long Total)> SearchAsync(
        string term, int page, int pageSize, CancellationToken ct = default);
}
