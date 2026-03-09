using ECommerce.Contracts.Products;
using ECommerce.ProductService.Domain;
using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Messaging;
using ECommerce.SharedKernel.Results;

namespace ECommerce.ProductService.Features.CreateProduct;

internal sealed class CreateProductHandler(IProductRepository repository, IEventBus eventBus)
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

        var integrationEvent = new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Description,
            product.CategoryId,
            product.BasePrice,
            product.Attributes
                .Select(a => new ProductAttributeDto(a.Key, a.Value, a.Unit))
                .ToList(),
            DateTimeOffset.UtcNow);

        await eventBus.PublishAsync(integrationEvent, ct);

        return Result.Success(product.Id);
    }
}
