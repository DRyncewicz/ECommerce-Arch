using ECommerce.SharedKernel.CQRS;

namespace ECommerce.SearchService.Features.SearchProducts;

internal sealed record SearchProductsQuery(
    string Term,
    int Page = 1,
    int PageSize = 20) : IQuery<SearchProductsResult>;

internal sealed record SearchProductsResult(
    IReadOnlyList<ProductSearchItem> Items,
    long Total,
    int Page,
    int PageSize);

internal sealed record ProductSearchItem(
    Guid Id,
    string Name,
    string Description,
    string CategoryId,
    decimal BasePrice,
    IReadOnlyList<AttributeItem> Attributes);

internal sealed record AttributeItem(string Key, string Value, string? Unit);
