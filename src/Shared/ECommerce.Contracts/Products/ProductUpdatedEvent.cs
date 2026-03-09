using ECommerce.SharedKernel.Messaging;

namespace ECommerce.Contracts.Products;

public sealed record ProductUpdatedEvent(
    Guid Id,
    string Name,
    decimal BasePrice,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
