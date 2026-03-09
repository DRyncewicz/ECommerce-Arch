using ECommerce.ProductService.Domain;
using ECommerce.ProductService.Features.GetProduct;
using ECommerce.ProductService.Infrastructure.Persistence;
using FluentAssertions;
using NSubstitute;

namespace ECommerce.ProductService.Tests.Features;

public class GetProductHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly GetProductHandler _sut;

    public GetProductHandlerTests()
    {
        _sut = new GetProductHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ExistingProduct_ReturnsProductResponse()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create(productId, "Test", "Desc", 9.99m, "cat-1");
        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.HandleAsync(new GetProductQuery(productId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(productId);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task HandleAsync_NonExistingProduct_ReturnsNotFoundFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var result = await _sut.HandleAsync(new GetProductQuery(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }
}
