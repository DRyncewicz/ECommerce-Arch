using ECommerce.SharedKernel.Domain;

namespace ECommerce.OrderService.Domain;

public sealed class OrderItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { }

    public static OrderItem Create(Guid id, Guid productId, string productName, int quantity, decimal unitPrice)
        => new()
        {
            Id = id,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
}
