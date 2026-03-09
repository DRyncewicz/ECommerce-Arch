using ECommerce.SharedKernel.Messaging;

namespace ECommerce.Contracts.Orders;

public sealed record OrderCancelledEvent(
    Guid Id,
    string Reason,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
