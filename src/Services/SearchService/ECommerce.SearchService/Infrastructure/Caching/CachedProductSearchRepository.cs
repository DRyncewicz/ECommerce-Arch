using System.Text.Json;
using ECommerce.SearchService.Infrastructure.Persistence;
using ECommerce.SearchService.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ECommerce.SearchService.Infrastructure.Caching;

internal sealed class CachedProductSearchRepository(
    IProductSearchRepository inner,
    IConnectionMultiplexer redis,
    IOptions<CacheSettings> cacheSettings,
    ILogger<CachedProductSearchRepository> logger) : IProductSearchRepository
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(cacheSettings.Value.SearchTtlSeconds);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task IndexAsync(ProductDocument document, CancellationToken ct = default)
        => inner.IndexAsync(document, ct);

    public async Task<(IReadOnlyList<ProductDocument> Hits, long Total)> SearchAsync(
        string term, int page, int pageSize, CancellationToken ct = default)
    {
        var key = BuildCacheKey(term, page, pageSize);
        try
        {
            var cached = await _db.StringGetAsync(key);
            if (cached.HasValue)
            {
                logger.LogDebug("Cache HIT {CacheKey}", key);
                var r = JsonSerializer.Deserialize<CachedResult>((string)cached!, JsonOptions);
                if (r is not null) return (r.Hits, r.Total);
            }
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis unavailable for {CacheKey}, falling through", key);
        }

        logger.LogDebug("Cache MISS {CacheKey}", key);
        var (hits, total) = await inner.SearchAsync(term, page, pageSize, ct);

        try
        {
            var payload = JsonSerializer.Serialize(new CachedResult(hits.ToList(), total), JsonOptions);
            await _db.StringSetAsync(key, payload, _ttl);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Failed to write cache for {CacheKey}", key);
        }

        return (hits, total);
    }

    private static string BuildCacheKey(string term, int page, int pageSize)
        => $"search:products:{term.Trim().ToLowerInvariant()}:{page}:{pageSize}";

    private sealed record CachedResult(List<ProductDocument> Hits, long Total);
}
