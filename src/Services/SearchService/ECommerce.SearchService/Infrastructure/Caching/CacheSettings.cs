namespace ECommerce.SearchService.Infrastructure.Caching;

internal sealed class CacheSettings
{
    public const string SectionName = "CacheSettings";
    public int SearchTtlSeconds { get; init; } = 300;
}
