using ECommerce.Contracts.Products;

namespace ECommerce.SearchService.Models;

internal sealed class ProductDocument
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public List<AttributeEntry> Attributes { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }

    public sealed class AttributeEntry
    {
        public string Key { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string? Unit { get; init; }
    }

    public static ProductDocument FromEvent(ProductCreatedEvent e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        CategoryId = e.CategoryId,
        BasePrice = e.BasePrice,
        Attributes = e.Attributes
            .Select(a => new AttributeEntry { Key = a.Key, Value = a.Value, Unit = a.Unit })
            .ToList(),
        CreatedAt = e.OccurredAt
    };
}
