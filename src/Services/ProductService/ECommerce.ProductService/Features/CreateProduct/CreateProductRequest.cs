namespace ECommerce.ProductService.Features.CreateProduct;

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal BasePrice,
    string CategoryId);
