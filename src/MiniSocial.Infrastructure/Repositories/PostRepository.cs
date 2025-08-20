using MongoDB.Driver;
using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;

namespace MiniSocial.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(IMongoDatabase database) : base(database, "posts")
    {
    }

    public async Task<IEnumerable<Post>> GetByAuthorIdAsync(string authorId)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.AuthorId, authorId);
        var sort = Builders<Post>.Sort.Descending(p => p.CreatedAt);
        
        return await _collection.Find(filter)
            .Sort(sort)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetFeedAsync(IEnumerable<string> followedUserIds, int skip = 0, int take = 20)
    {
        var filter = Builders<Post>.Filter.In(p => p.AuthorId, followedUserIds);
        var sort = Builders<Post>.Sort.Descending(p => p.CreatedAt);
        
        return await _collection.Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByHashtagAsync(string hashtag, int skip = 0, int take = 20)
    {
        var filter = Builders<Post>.Filter.AnyIn(p => p.Hashtags, new[] { hashtag.ToLowerInvariant() });
        var sort = Builders<Post>.Sort.Descending(p => p.CreatedAt);
        
        return await _collection.Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> SearchAsync(string searchTerm, int skip = 0, int take = 20)
    {
        var filter = Builders<Post>.Filter.Text(searchTerm);
        var sort = Builders<Post>.Sort.MetaTextScore("textScore");
        
        return await _collection.Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetTrendingAsync(int hours = 24, int skip = 0, int take = 20)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        var filter = Builders<Post>.Filter.Gte(p => p.CreatedAt, cutoffTime);
        var sort = Builders<Post>.Sort.Combine(
            Builders<Post>.Sort.Descending(p => p.LikesCount),
            Builders<Post>.Sort.Descending(p => p.CommentsCount),
            Builders<Post>.Sort.Descending(p => p.CreatedAt)
        );
        
        return await _collection.Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }
}
