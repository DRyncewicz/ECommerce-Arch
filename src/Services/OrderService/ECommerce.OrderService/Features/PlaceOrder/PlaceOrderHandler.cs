using ECommerce.OrderService.Domain;
using ECommerce.OrderService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Results;

namespace ECommerce.OrderService.Features.PlaceOrder;

internal sealed class PlaceOrderHandler(IOrderRepository repository)
    : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(PlaceOrderCommand command, CancellationToken ct = default)
    {
        var items = command.Items.Select(i =>
            OrderItem.Create(Guid.NewGuid(), i.ProductId, i.ProductName, i.Quantity, i.UnitPrice));

        var order = Order.Create(Guid.NewGuid(), command.CustomerId, items);
        await repository.AddAsync(order, ct);
        return Result.Success(order.Id);
    }
}
