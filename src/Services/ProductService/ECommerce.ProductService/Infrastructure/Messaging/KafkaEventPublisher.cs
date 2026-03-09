using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Contracts.Products;
using ECommerce.SharedKernel.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.ProductService.Infrastructure.Messaging;

internal sealed class KafkaEventPublisher : IEventBus, IDisposable
{
    private static readonly Dictionary<Type, string> TopicMap = new()
    {
        [typeof(ProductCreatedEvent)] = ECommerce.Contracts.Topics.ProductCreated,
        [typeof(ProductUpdatedEvent)] = ECommerce.Contracts.Topics.ProductUpdated,
    };

    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : IIntegrationEvent
    {
        if (!TopicMap.TryGetValue(typeof(T), out var topic))
        {
            _logger.LogWarning("No topic registered for event type {EventType}", typeof(T).Name);
            return;
        }

        var json = JsonSerializer.Serialize(integrationEvent);
        var message = new Message<string, string>
        {
            Key = integrationEvent.Id.ToString(),
            Value = json
        };

        var result = await _producer.ProduceAsync(topic, message, ct);
        _logger.LogDebug("Published {EventType} to {Topic} [{Partition}@{Offset}]",
            typeof(T).Name, topic, result.Partition.Value, result.Offset.Value);
    }

    public void Dispose() => _producer.Dispose();
}
