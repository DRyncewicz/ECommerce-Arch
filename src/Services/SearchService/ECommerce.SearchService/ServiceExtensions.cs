using Elastic.Clients.Elasticsearch;
using ECommerce.SearchService.Features.SearchProducts;
using ECommerce.SearchService.Infrastructure.Messaging;
using ECommerce.SearchService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;

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
        services.AddScoped<IProductSearchRepository, ElasticsearchProductRepository>();

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
