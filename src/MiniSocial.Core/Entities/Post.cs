using System.Text.RegularExpressions;

namespace MiniSocial.Core.Entities;

public class Post
{
    private const int MaxContentLength = 500;
    private const int MaxImagesPerPost = 5;
    
    private static readonly Regex HashtagRegex = new Regex(
        @"#(\w+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex MentionRegex = new Regex(
        @"@(\w+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Id { get; private set; }
    public string AuthorId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int LikesCount { get; private set; }
    public int CommentsCount { get; private set; }
    public List<string> ImageUrls { get; private set; }
    public List<string> Hashtags { get; private set; }
    public List<string> Mentions { get; private set; }

    public Post(string id, string authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or whitespace", nameof(id));
        
        if (string.IsNullOrWhiteSpace(authorId))
            throw new ArgumentException("Author ID cannot be null or whitespace", nameof(authorId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
        
        if (content.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot exceed {MaxContentLength} characters", nameof(content));

        Id = id;
        AuthorId = authorId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        LikesCount = 0;
        CommentsCount = 0;
        ImageUrls = new List<string>();
        
        // Extract hashtags and mentions from content
        Hashtags = ExtractHashtags(content);
        Mentions = ExtractMentions(content);
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or whitespace", nameof(content));
        
        if (content.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot exceed {MaxContentLength} characters", nameof(content));

        Content = content;
        UpdatedAt = DateTime.UtcNow;
        
        // Re-extract hashtags and mentions
        Hashtags = ExtractHashtags(content);
        Mentions = ExtractMentions(content);
    }

    public void AddImageUrls(IEnumerable<string> imageUrls)
    {
        if (imageUrls == null)
            throw new ArgumentNullException(nameof(imageUrls));

        var urlsToAdd = imageUrls.ToList();
        
        if (ImageUrls.Count + urlsToAdd.Count > MaxImagesPerPost)
            throw new ArgumentException($"Cannot add more than {MaxImagesPerPost} images per post", nameof(imageUrls));

        ImageUrls.AddRange(urlsToAdd);
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

    public void IncrementCommentsCount()
    {
        CommentsCount++;
    }

    public void DecrementCommentsCount()
    {
        if (CommentsCount <= 0)
            throw new InvalidOperationException("Comments count cannot be negative");
        
        CommentsCount--;
    }

    private static List<string> ExtractHashtags(string content)
    {
        return HashtagRegex.Matches(content)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    private static List<string> ExtractMentions(string content)
    {
        return MentionRegex.Matches(content)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .ToList();
    }
}
