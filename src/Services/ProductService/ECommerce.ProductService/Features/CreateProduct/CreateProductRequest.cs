namespace ECommerce.ProductService.Features.CreateProduct;

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal BasePrice,
    string CategoryId,
    List<ProductAttributeRequest>? Attributes = null);

public sealed record ProductAttributeRequest(string Key, string Value, string? Unit = null);
