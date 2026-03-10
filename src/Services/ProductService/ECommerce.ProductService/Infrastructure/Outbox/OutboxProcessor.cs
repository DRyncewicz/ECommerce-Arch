using System.Text.Json;
using ECommerce.Contracts.Products;
using ECommerce.ProductService.Domain;
using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.SharedKernel.Messaging;

namespace ECommerce.ProductService.Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingEventsAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        IReadOnlyList<Product> products;
        try
        {
            products = await repository.GetWithPendingOutboxEventsAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Outbox: failed to fetch products with pending events");
            return;
        }

        foreach (var product in products)
        {
            foreach (var outboxEvent in product.OutboxEvents)
            {
                await PublishAndRemoveAsync(repository, product.Id, outboxEvent, ct);
            }
        }
    }

    private async Task PublishAndRemoveAsync(
        IProductRepository repository,
        Guid productId,
        OutboxEvent outboxEvent,
        CancellationToken ct)
    {
        try
        {
            await PublishEventAsync(outboxEvent, ct);
            await repository.RemoveOutboxEventAsync(productId, outboxEvent.Id, ct);

            logger.LogInformation(
                "Outbox: published {EventType} {EventId} for product {ProductId}",
                outboxEvent.EventType, outboxEvent.Id, productId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Outbox: failed to publish {EventType} {EventId} for product {ProductId}",
                outboxEvent.EventType, outboxEvent.Id, productId);
        }
    }

    private async Task PublishEventAsync(OutboxEvent outboxEvent, CancellationToken ct)
    {
        switch (outboxEvent.EventType)
        {
            case nameof(ProductCreatedEvent):
                var evt = JsonSerializer.Deserialize<ProductCreatedEvent>(outboxEvent.Payload)
                    ?? throw new InvalidOperationException($"Failed to deserialize {outboxEvent.EventType}");
                await eventBus.PublishAsync(evt, ct);
                break;

            default:
                logger.LogWarning("Outbox: unknown event type {EventType}, skipping", outboxEvent.EventType);
                break;
        }
    }
}
