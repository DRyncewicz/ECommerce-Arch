namespace ECommerce.Contracts;

public static class Topics
{
    public const string ProductCreated = "product.created";
    public const string ProductUpdated = "product.updated";
    public const string OrderPlaced    = "order.placed";
    public const string OrderPaid      = "order.paid";
    public const string OrderShipped   = "order.shipped";
    public const string OrderCancelled = "order.cancelled";
    public const string CartAbandoned  = "cart.abandoned";
    public const string UserRegistered = "user.registered";
    public const string InventoryLow   = "inventory.low";
    public const string PaymentFailed  = "payment.failed";
}
