using ECommerce.SharedKernel.Domain;

namespace ECommerce.ProductService.Domain;

public sealed class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal BasePrice { get; private set; }
    public string CategoryId { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public List<ProductVariant> Variants { get; private set; } = [];
    public List<ProductAttribute> Attributes { get; private set; } = [];

    private Product() { }

    public static Product Create(
        Guid id,
        string name,
        string description,
        decimal basePrice,
        string categoryId,
        IEnumerable<ProductAttribute>? attributes = null)
    {
        var product = new Product
        {
            Id = id,
            Name = name,
            Description = description,
            BasePrice = basePrice,
            CategoryId = categoryId,
            IsActive = true,
            Attributes = attributes?.ToList() ?? []
        };
        product.RaiseDomainEvent(new ProductCreatedDomainEvent(id, name, categoryId, basePrice));
        return product;
    }

    public void Update(string name, string description, decimal basePrice)
    {
        Name = name;
        Description = description;
        BasePrice = basePrice;
        RaiseDomainEvent(new ProductUpdatedDomainEvent(Id, name, basePrice));
    }

    public void Deactivate() => IsActive = false;
}

public sealed record ProductCreatedDomainEvent(Guid ProductId, string Name, string CategoryId, decimal BasePrice)
    : IDomainEvent;

public sealed record ProductUpdatedDomainEvent(Guid ProductId, string Name, decimal BasePrice)
    : IDomainEvent;
