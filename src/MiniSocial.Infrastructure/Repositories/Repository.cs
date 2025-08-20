using MongoDB.Driver;
using MiniSocial.Core.Interfaces;

namespace MiniSocial.Infrastructure.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    protected Repository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Entity {typeof(T).Name} must have an Id property");

        var id = idProperty.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Entity Id cannot be null or empty");

        var filter = Builders<T>.Filter.Eq("_id", id);
        await _collection.ReplaceOneAsync(filter, entity);
        return entity;
    }
}
