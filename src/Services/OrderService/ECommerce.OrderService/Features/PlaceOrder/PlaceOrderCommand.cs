using ECommerce.SharedKernel.CQRS;

namespace ECommerce.OrderService.Features.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    List<OrderItemDto> Items) : ICommand<Guid>;

public sealed record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
