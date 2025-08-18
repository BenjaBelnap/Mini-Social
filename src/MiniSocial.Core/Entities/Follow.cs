namespace MiniSocial.Core.Entities;

public class Follow
{
    public string Id { get; private set; }
    public string FollowerId { get; private set; }
    public string FolloweeId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Follow(string id, string followerId, string followeeId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or whitespace", nameof(id));
        
        if (string.IsNullOrWhiteSpace(followerId))
            throw new ArgumentException("Follower ID cannot be null or whitespace", nameof(followerId));
        
        if (string.IsNullOrWhiteSpace(followeeId))
            throw new ArgumentException("Followee ID cannot be null or whitespace", nameof(followeeId));
        
        if (followerId.Equals(followeeId, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("User cannot follow themselves", nameof(followeeId));

        Id = id;
        FollowerId = followerId;
        FolloweeId = followeeId;
        CreatedAt = DateTime.UtcNow;
    }
}
