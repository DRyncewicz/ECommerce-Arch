using ECommerce.SharedKernel.Messaging;

namespace ECommerce.Contracts.Inventory;

public sealed record StockReservedEvent(
    Guid Id,
    Guid ProductId,
    Guid OrderId,
    int Quantity,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
