using ECommerce.SharedKernel.CQRS;

namespace ECommerce.ProductService.Features.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal BasePrice,
    string CategoryId) : ICommand<Guid>;
