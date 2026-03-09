using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Results;

namespace ECommerce.ProductService.Features.GetProduct;

internal sealed class GetProductHandler(IProductRepository repository)
    : IQueryHandler<GetProductQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> HandleAsync(GetProductQuery query, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(query.ProductId, ct);

        if (product is null)
            return Result.Failure<ProductResponse>(Error.NotFound("Product"));

        return Result.Success(new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.BasePrice,
            product.CategoryId,
            product.IsActive));
    }
}
