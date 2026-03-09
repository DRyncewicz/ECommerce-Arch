using ECommerce.SharedKernel.Domain;

namespace ECommerce.ProductService.Domain;

public sealed class Category : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid? ParentCategoryId { get; private set; }

    private Category() { }

    public static Category Create(Guid id, string name, string slug, Guid? parentCategoryId = null)
        => new() { Id = id, Name = name, Slug = slug, ParentCategoryId = parentCategoryId };
}
