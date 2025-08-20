using MongoDB.Driver;
using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;

namespace MiniSocial.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(IMongoDatabase database) : base(database, "comments")
    {
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(string postId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
        var sort = Builders<Comment>.Sort.Ascending(c => c.CreatedAt);
        
        return await _collection.Find(filter)
            .Sort(sort)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetRepliesByParentIdAsync(string parentCommentId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.ParentCommentId, parentCommentId);
        var sort = Builders<Comment>.Sort.Ascending(c => c.CreatedAt);
        
        return await _collection.Find(filter)
            .Sort(sort)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.AuthorId, authorId);
        var sort = Builders<Comment>.Sort.Descending(c => c.CreatedAt);
        
        return await _collection.Find(filter)
            .Sort(sort)
            .ToListAsync();
    }

    public async Task<int> GetCommentCountByPostIdAsync(string postId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.PostId, postId);
        return (int)await _collection.CountDocumentsAsync(filter);
    }
}
