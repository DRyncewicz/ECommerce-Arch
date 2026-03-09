using ECommerce.ProductService.Domain;
using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Results;

namespace ECommerce.ProductService.Features.CreateProduct;

internal sealed class CreateProductHandler(IProductRepository repository)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateProductCommand command, CancellationToken ct = default)
    {
        var product = Product.Create(
            Guid.NewGuid(),
            command.Name,
            command.Description,
            command.BasePrice,
            command.CategoryId,
            command.Attributes);

        await repository.AddAsync(product, ct);
        return Result.Success(product.Id);
    }
}
