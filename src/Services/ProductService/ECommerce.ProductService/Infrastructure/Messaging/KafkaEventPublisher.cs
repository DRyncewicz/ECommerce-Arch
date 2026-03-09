using ECommerce.SharedKernel.Messaging;

namespace ECommerce.ProductService.Infrastructure.Messaging;

// TODO: Replace with Confluent.Kafka producer once outbox pattern is in place.
internal sealed class KafkaEventPublisher : IEventBus
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : IIntegrationEvent
    {
        return Task.CompletedTask;
    }
}
