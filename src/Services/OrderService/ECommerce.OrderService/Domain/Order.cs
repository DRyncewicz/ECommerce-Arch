using ECommerce.SharedKernel.Domain;
using ECommerce.SharedKernel.Results;

namespace ECommerce.OrderService.Domain;

public sealed class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public DateTimeOffset PlacedAt { get; private set; }

    private Order() { }

    public static Order Create(Guid id, Guid customerId, IEnumerable<OrderItem> items)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            Items = items.ToList(),
            PlacedAt = DateTimeOffset.UtcNow
        };
        order.RaiseDomainEvent(new OrderPlacedDomainEvent(id, customerId, order.TotalAmount));
        return order;
    }

    public Result Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            return Result.Failure(new Error("Order.CannotCancel", "Cannot cancel an order that has been shipped or delivered."));

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, reason));
        return Result.Success();
    }
}

public sealed record OrderPlacedDomainEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount) : IDomainEvent;
public sealed record OrderCancelledDomainEvent(Guid OrderId, string Reason) : IDomainEvent;
