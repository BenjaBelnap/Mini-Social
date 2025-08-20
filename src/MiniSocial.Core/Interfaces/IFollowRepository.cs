using MiniSocial.Core.Entities;

namespace MiniSocial.Core.Interfaces;

public interface IFollowRepository : IRepository<Follow>
{
    Task<IEnumerable<Follow>> GetFollowersByUserIdAsync(string userId);
    Task<IEnumerable<Follow>> GetFollowingByUserIdAsync(string userId);
    Task<bool> IsFollowingAsync(string followerId, string followeeId);
    Task<Follow?> GetFollowRelationshipAsync(string followerId, string followeeId);
    Task<int> GetFollowersCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
}
