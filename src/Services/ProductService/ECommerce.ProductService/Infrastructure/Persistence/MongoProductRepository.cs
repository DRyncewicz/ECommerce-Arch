using ECommerce.ProductService.Domain;
using MongoDB.Driver;

namespace ECommerce.ProductService.Infrastructure.Persistence;

internal sealed class MongoProductRepository(IMongoDatabase database) : IProductRepository
{
    private readonly IMongoCollection<Product> _collection = database.GetCollection<Product>("products");

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _collection.Find(p => p.Id == id).FirstOrDefaultAsync(ct)!;

    public async Task<IReadOnlyList<Product>> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var results = await _collection.Find(_ => true)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
        return results;
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
        => _collection.InsertOneAsync(product, cancellationToken: ct);

    public Task UpdateAsync(Product product, CancellationToken ct = default)
        => _collection.ReplaceOneAsync(p => p.Id == product.Id, product, cancellationToken: ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _collection.DeleteOneAsync(p => p.Id == id, ct);
}
