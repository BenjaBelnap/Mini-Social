using MongoDB.Driver;
using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;

namespace MiniSocial.Infrastructure.Repositories;

public class FollowRepository : Repository<Follow>, IFollowRepository
{
    public FollowRepository(IMongoDatabase database) : base(database, "follows")
    {
    }

    public async Task<IEnumerable<Follow>> GetFollowersByUserIdAsync(string userId)
    {
        var filter = Builders<Follow>.Filter.Eq(f => f.FolloweeId, userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Follow>> GetFollowingByUserIdAsync(string userId)
    {
        var filter = Builders<Follow>.Filter.Eq(f => f.FollowerId, userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followeeId)
    {
        var filter = Builders<Follow>.Filter.And(
            Builders<Follow>.Filter.Eq(f => f.FollowerId, followerId),
            Builders<Follow>.Filter.Eq(f => f.FolloweeId, followeeId)
        );
        
        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<Follow?> GetFollowRelationshipAsync(string followerId, string followeeId)
    {
        var filter = Builders<Follow>.Filter.And(
            Builders<Follow>.Filter.Eq(f => f.FollowerId, followerId),
            Builders<Follow>.Filter.Eq(f => f.FolloweeId, followeeId)
        );
        
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<int> GetFollowersCountAsync(string userId)
    {
        var filter = Builders<Follow>.Filter.Eq(f => f.FolloweeId, userId);
        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        var filter = Builders<Follow>.Filter.Eq(f => f.FollowerId, userId);
        return (int)await _collection.CountDocumentsAsync(filter);
    }
}
