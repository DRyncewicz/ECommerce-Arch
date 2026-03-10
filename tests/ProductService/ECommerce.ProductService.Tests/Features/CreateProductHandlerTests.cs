using ECommerce.ProductService.Features.CreateProduct;
using ECommerce.ProductService.Infrastructure.Persistence;
using FluentAssertions;
using NSubstitute;

namespace ECommerce.ProductService.Tests.Features;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly CreateProductHandler _sut;

    public CreateProductHandlerTests()
    {
        _sut = new CreateProductHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithId()
    {
        var command = new CreateProductCommand("Test Product", "A description", 9.99m, "cat-1", []);

        var result = await _sut.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<ECommerce.ProductService.Domain.Product>(p =>
                p.Name == "Test Product" && p.OutboxEvents.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreatesProductWithCorrectProperties()
    {
        var command = new CreateProductCommand("Widget", "A widget", 19.99m, "widgets", []);

        var result = await _sut.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddAsync(
            Arg.Is<ECommerce.ProductService.Domain.Product>(p =>
                p.Name == "Widget" && p.BasePrice == 19.99m && p.CategoryId == "widgets"),
            Arg.Any<CancellationToken>());
    }
}
