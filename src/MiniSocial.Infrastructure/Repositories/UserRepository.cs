using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;
using MongoDB.Driver;

namespace MiniSocial.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(IMongoDatabase database) : base(database, "users")
    {
        // Create indexes for better performance
        CreateIndexes();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var count = await _collection.CountDocumentsAsync(u => u.Email == email);
        return count == 0;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        var count = await _collection.CountDocumentsAsync(u => u.Username == username);
        return count == 0;
    }

    public async Task<IEnumerable<User>> SearchByUsernameAsync(string searchTerm)
    {
        var filter = Builders<User>.Filter.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
        return await _collection.Find(filter).Limit(20).ToListAsync();
    }

    private void CreateIndexes()
    {
        var usernameIndex = Builders<User>.IndexKeys.Ascending(u => u.Username);
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);
        
        _collection.Indexes.CreateOne(new CreateIndexModel<User>(usernameIndex, 
            new CreateIndexOptions { Unique = true }));
        _collection.Indexes.CreateOne(new CreateIndexModel<User>(emailIndex, 
            new CreateIndexOptions { Unique = true }));
    }
}
