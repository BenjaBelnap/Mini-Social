namespace MiniSocial.Core.Entities;

public class Comment
{
    private const int MaxContentLength = 1000;

    public string Id { get; private set; }
    public string PostId { get; private set; }
    public string AuthorId { get; private set; }
    public string Content { get; private set; }
    public string? ParentCommentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int LikesCount { get; private set; }

    public Comment(string id, string postId, string authorId, string content, string? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or whitespace", nameof(id));
        
        if (string.IsNullOrWhiteSpace(postId))
            throw new ArgumentException("Post ID cannot be null or whitespace", nameof(postId));
        
        if (string.IsNullOrWhiteSpace(authorId))
            throw new ArgumentException("Author ID cannot be null or whitespace", nameof(authorId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
        
        if (content.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot exceed {MaxContentLength} characters", nameof(content));

        Id = id;
        PostId = postId;
        AuthorId = authorId;
        Content = content;
        ParentCommentId = parentCommentId;
        CreatedAt = DateTime.UtcNow;
        LikesCount = 0;
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
        
        if (content.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot exceed {MaxContentLength} characters", nameof(content));

        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementLikesCount()
    {
        LikesCount++;
    }

    public void DecrementLikesCount()
    {
        if (LikesCount <= 0)
            throw new InvalidOperationException("Likes count cannot be negative");
        
        LikesCount--;
    }
}
