using ECommerce.SharedKernel.Messaging;

namespace ECommerce.Contracts.Orders;

public sealed record OrderPlacedEvent(
    Guid Id,
    Guid CustomerId,
    decimal TotalAmount,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
