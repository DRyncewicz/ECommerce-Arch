using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ECommerce.SearchService.Models;
using Microsoft.Extensions.Logging;

namespace ECommerce.SearchService.Infrastructure.Persistence;

internal sealed class ElasticsearchProductRepository(
    ElasticsearchClient client,
    ILogger<ElasticsearchProductRepository> logger) : IProductSearchRepository
{
    private const string IndexName = "products";

    public async Task IndexAsync(ProductDocument document, CancellationToken ct = default)
    {
        var response = await client.IndexAsync(document, i => i
            .Index(IndexName)
            .Id(document.Id.ToString()), ct);

        if (!response.IsValidResponse)
        {
            var reason = response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation;
            logger.LogError("Failed to index product {ProductId}: {Error}", document.Id, reason);
            throw new InvalidOperationException($"Elasticsearch index failed: {reason}");
        }
    }

    public async Task<(IReadOnlyList<ProductDocument> Hits, long Total)> SearchAsync(
        string term, int page, int pageSize, CancellationToken ct = default)
    {
        var from = (page - 1) * pageSize;

        SearchResponse<ProductDocument> response;

        if (string.IsNullOrWhiteSpace(term))
        {
            response = await client.SearchAsync<ProductDocument>(s => s
                .Indices(IndexName)
                .From(from)
                .Size(pageSize)
                .Query(q => q.MatchAll(new MatchAllQuery())), ct);
        }
        else
        {
            response = await client.SearchAsync<ProductDocument>(s => s
                .Indices(IndexName)
                .From(from)
                .Size(pageSize)
                .Query(q => q.MultiMatch(mm => mm
                    .Query(term)
                    .Fields(new[] { "name^3", "description", "categoryId", "attributes.key", "attributes.value" })
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(new Fuzziness("AUTO"))
                )), ct);
        }

        if (!response.IsValidResponse)
        {
            logger.LogError("Elasticsearch search failed: {Error}",
                response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation);
            return ([], 0);
        }

        var total = response.HitsMetadata?.Total?.Match(hits => hits.Value, count => count) ?? 0L;
        return (response.Documents.ToList(), total);
    }
}
