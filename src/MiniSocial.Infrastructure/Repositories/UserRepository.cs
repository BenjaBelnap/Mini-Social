using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;
using MiniSocial.Infrastructure.Configuration;
using MongoDB.Driver;

namespace MiniSocial.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _users = database.GetCollection<User>("users");
        
        // Create indexes for better performance
        CreateIndexes();
    }

    public async Task<User> CreateAsync(User entity)
    {
        await _users.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _users.CountDocumentsAsync(u => u.Id == id);
        return count > 0;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _users.Find(_ => true).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var count = await _users.CountDocumentsAsync(u => u.Email == email);
        return count == 0;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        var count = await _users.CountDocumentsAsync(u => u.Username == username);
        return count == 0;
    }

    public async Task<IEnumerable<User>> SearchByUsernameAsync(string searchTerm)
    {
        var filter = Builders<User>.Filter.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
        return await _users.Find(filter).Limit(20).ToListAsync();
    }

    public async Task<User> UpdateAsync(User entity)
    {
        await _users.ReplaceOneAsync(u => u.Id == entity.Id, entity);
        return entity;
    }

    private void CreateIndexes()
    {
        var usernameIndex = Builders<User>.IndexKeys.Ascending(u => u.Username);
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);
        
        _users.Indexes.CreateOne(new CreateIndexModel<User>(usernameIndex, 
            new CreateIndexOptions { Unique = true }));
        _users.Indexes.CreateOne(new CreateIndexModel<User>(emailIndex, 
            new CreateIndexOptions { Unique = true }));
    }
}
