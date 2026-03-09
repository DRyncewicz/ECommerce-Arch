using ECommerce.SharedKernel.Domain;

namespace ECommerce.ProductService.Domain;

public sealed class ProductVariant : Entity<Guid>
{
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal PriceModifier { get; private set; }
    public int StockQuantity { get; private set; }

    private ProductVariant() { }

    public static ProductVariant Create(Guid id, string sku, string name, decimal priceModifier, int stockQuantity)
        => new() { Id = id, Sku = sku, Name = name, PriceModifier = priceModifier, StockQuantity = stockQuantity };
}
