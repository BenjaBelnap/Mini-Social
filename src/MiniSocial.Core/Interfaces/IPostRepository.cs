using MiniSocial.Core.Entities;

namespace MiniSocial.Core.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetByAuthorIdAsync(string authorId);
    Task<IEnumerable<Post>> GetFeedAsync(IEnumerable<string> followedUserIds, int skip = 0, int take = 20);
    Task<IEnumerable<Post>> GetByHashtagAsync(string hashtag, int skip = 0, int take = 20);
    Task<IEnumerable<Post>> SearchAsync(string searchTerm, int skip = 0, int take = 20);
    Task<IEnumerable<Post>> GetTrendingAsync(int hours = 24, int skip = 0, int take = 20);
}
