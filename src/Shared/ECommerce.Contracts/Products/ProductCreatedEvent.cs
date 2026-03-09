using ECommerce.SharedKernel.Messaging;

namespace ECommerce.Contracts.Products;

public sealed record ProductCreatedEvent(
    Guid Id,
    string Name,
    string CategoryId,
    decimal BasePrice,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
