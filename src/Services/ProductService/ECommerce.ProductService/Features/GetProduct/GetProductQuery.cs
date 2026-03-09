using ECommerce.SharedKernel.CQRS;

namespace ECommerce.ProductService.Features.GetProduct;

public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductResponse>;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal BasePrice,
    string CategoryId,
    bool IsActive);
