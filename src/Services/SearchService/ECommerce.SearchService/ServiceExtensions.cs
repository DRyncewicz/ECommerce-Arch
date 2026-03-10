using Elastic.Clients.Elasticsearch;
using ECommerce.SearchService.Features.SearchProducts;
using ECommerce.SearchService.Infrastructure.Caching;
using ECommerce.SearchService.Infrastructure.Messaging;
using ECommerce.SearchService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ECommerce.SearchService;

public static class ServiceExtensions
{
    public static IServiceCollection AddSearchServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Elasticsearch
        var elasticsearchUrl = configuration.GetConnectionString("Elasticsearch")
            ?? "http://elasticsearch:9200";
        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl))
            .DefaultIndex("products");
        services.AddSingleton(new ElasticsearchClient(settings));

        // Cache settings
        services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));

        // Redis — singleton (thread-safe, manages connection pooling)
        var redisConnection = configuration.GetConnectionString("Redis") ?? "redis:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));

        // IProductSearchRepository with cache decorator
        services.AddScoped<ElasticsearchProductRepository>();
        services.AddScoped<IProductSearchRepository>(sp =>
            new CachedProductSearchRepository(
                sp.GetRequiredService<ElasticsearchProductRepository>(),
                sp.GetRequiredService<IConnectionMultiplexer>(),
                sp.GetRequiredService<IOptions<CacheSettings>>(),
                sp.GetRequiredService<ILogger<CachedProductSearchRepository>>()));

        // CQRS dispatcher
        services.AddSingleton<Dispatcher>();
        services.AddSingleton<IQueryDispatcher>(sp => sp.GetRequiredService<Dispatcher>());

        // Handlers
        services.AddScoped<
            IQueryHandler<SearchProductsQuery, SearchProductsResult>,
            SearchProductsHandler>();

        // Kafka consumers
        services.AddHostedService<ProductCreatedConsumer>();

        return services;
    }
}
