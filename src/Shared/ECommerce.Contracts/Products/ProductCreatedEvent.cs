using ECommerce.SharedKernel.Messaging;

namespace ECommerce.Contracts.Products;

public sealed record ProductCreatedEvent(
    Guid Id,
    string Name,
    string Description,
    string CategoryId,
    decimal BasePrice,
    IReadOnlyList<ProductAttributeDto> Attributes,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
