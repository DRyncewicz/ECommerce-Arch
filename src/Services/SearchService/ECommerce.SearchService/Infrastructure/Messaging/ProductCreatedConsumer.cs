using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Contracts;
using ECommerce.Contracts.Products;
using ECommerce.SearchService.Infrastructure.Persistence;
using ECommerce.SearchService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.SearchService.Infrastructure.Messaging;

internal sealed class ProductCreatedConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ProductCreatedConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => RunConsumerLoop(stoppingToken), stoppingToken);

    private void RunConsumerLoop(CancellationToken ct)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
            GroupId = "search-service-product-created",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(Topics.ProductCreated);
        logger.LogInformation("ProductCreatedConsumer started, subscribed to {Topic}", Topics.ProductCreated);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null) continue;

                var @event = JsonSerializer.Deserialize<ProductCreatedEvent>(result.Message.Value);
                if (@event is null)
                {
                    logger.LogWarning("Failed to deserialize ProductCreatedEvent from message");
                    consumer.Commit(result);
                    continue;
                }

                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IProductSearchRepository>();
                repository.IndexAsync(ProductDocument.FromEvent(@event), ct).GetAwaiter().GetResult();

                consumer.Commit(result);
                logger.LogInformation("Indexed product {ProductId} ({Name})", @event.Id, @event.Name);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex) when (ex.Error.IsFatal)
            {
                logger.LogCritical(ex, "Fatal Kafka consume error, stopping consumer");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing {Topic} message", Topics.ProductCreated);
            }
        }

        consumer.Close();
        logger.LogInformation("ProductCreatedConsumer stopped");
    }
}
