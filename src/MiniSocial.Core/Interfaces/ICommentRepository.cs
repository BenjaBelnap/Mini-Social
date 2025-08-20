using MiniSocial.Core.Entities;

namespace MiniSocial.Core.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByPostIdAsync(string postId);
    Task<IEnumerable<Comment>> GetRepliesByParentIdAsync(string parentCommentId);
    Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId);
    Task<int> GetCommentCountByPostIdAsync(string postId);
}
