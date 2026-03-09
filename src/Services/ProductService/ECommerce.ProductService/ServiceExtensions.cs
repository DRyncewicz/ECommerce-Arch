using ECommerce.ProductService.Infrastructure.Messaging;
using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.SharedKernel.CQRS;
using ECommerce.SharedKernel.Messaging;
using FluentValidation;
using MongoDB.Driver;

namespace ECommerce.ProductService;

public static class ServiceExtensions
{
    public static IServiceCollection AddProductServices(this IServiceCollection services, IConfiguration configuration)
    {
        // CQRS dispatcher
        services.AddSingleton<Dispatcher>();
        services.AddSingleton<ICommandDispatcher>(sp => sp.GetRequiredService<Dispatcher>());
        services.AddSingleton<IQueryDispatcher>(sp => sp.GetRequiredService<Dispatcher>());

        // Handlers (scoped so they can use scoped repos)
        services.AddScoped<
            ICommandHandler<Features.CreateProduct.CreateProductCommand, Guid>,
            Features.CreateProduct.CreateProductHandler>();
        services.AddScoped<
            IQueryHandler<Features.GetProduct.GetProductQuery, Features.GetProduct.ProductResponse>,
            Features.GetProduct.GetProductHandler>();

        // Validators
        services.AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly);

        // MongoDB
        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("products_db"));
        services.AddScoped<IProductRepository, MongoProductRepository>();

        // Event bus
        services.AddSingleton<IEventBus, KafkaEventPublisher>();

        return services;
    }
}
