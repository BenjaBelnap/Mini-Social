using FluentAssertions;
using MiniSocial.Core.Entities;
using Xunit;

namespace MiniSocial.Tests.Core.Entities;

public class PostTests
{
    [Fact]
    public void Post_Should_Be_Created_With_Valid_Properties()
    {
        // Arrange
        var postId = "post123";
        var authorId = "user123";
        var content = "This is a test post";

        // Act
        var post = new Post(postId, authorId, content);

        // Assert
        post.Id.Should().Be(postId);
        post.AuthorId.Should().Be(authorId);
        post.Content.Should().Be(content);
        post.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        post.UpdatedAt.Should().BeNull();
        post.LikesCount.Should().Be(0);
        post.CommentsCount.Should().Be(0);
        post.ImageUrls.Should().BeEmpty();
        post.Hashtags.Should().BeEmpty();
        post.Mentions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Post_Should_Throw_ArgumentException_When_Content_Is_Invalid(string? invalidContent)
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604 // Possible null reference argument - intentional for testing
        var action = () => new Post("post123", "user123", invalidContent);
#pragma warning restore CS8604
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or whitespace*");
    }

    [Fact]
    public void Post_Should_Throw_ArgumentException_When_Content_Exceeds_MaxLength()
    {
        // Arrange
        var longContent = new string('a', 501); // Assuming 500 character limit

        // Act & Assert
        var action = () => new Post("post123", "user123", longContent);
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot exceed 500 characters*");
    }

    [Fact]
    public void Post_Should_Allow_Updating_Content()
    {
        // Arrange
        var post = new Post("post123", "user123", "Original content");
        var newContent = "Updated content";

        // Act
        post.UpdateContent(newContent);

        // Assert
        post.Content.Should().Be(newContent);
        post.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Post_Should_Extract_Hashtags_From_Content()
    {
        // Arrange & Act
        var post = new Post("post123", "user123", "This is a #test post with #multiple #hashtags");

        // Assert
        post.Hashtags.Should().HaveCount(3);
        post.Hashtags.Should().Contain("test");
        post.Hashtags.Should().Contain("multiple");
        post.Hashtags.Should().Contain("hashtags");
    }

    [Fact]
    public void Post_Should_Extract_Mentions_From_Content()
    {
        // Arrange & Act
        var post = new Post("post123", "user123", "Hey @john_doe and @jane_smith, check this out!");

        // Assert
        post.Mentions.Should().HaveCount(2);
        post.Mentions.Should().Contain("john_doe");
        post.Mentions.Should().Contain("jane_smith");
    }

    [Fact]
    public void Post_Should_Add_Image_Urls()
    {
        // Arrange
        var post = new Post("post123", "user123", "Test post");
        var imageUrls = new[] { "https://example.com/image1.jpg", "https://example.com/image2.jpg" };

        // Act
        post.AddImageUrls(imageUrls);

        // Assert
        post.ImageUrls.Should().HaveCount(2);
        post.ImageUrls.Should().Contain("https://example.com/image1.jpg");
        post.ImageUrls.Should().Contain("https://example.com/image2.jpg");
    }

    [Fact]
    public void Post_Should_Limit_Maximum_Images()
    {
        // Arrange
        var post = new Post("post123", "user123", "Test post");
        var tooManyImages = Enumerable.Range(1, 6).Select(i => $"https://example.com/image{i}.jpg").ToArray();

        // Act & Assert
        var action = () => post.AddImageUrls(tooManyImages);
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Cannot add more than 5 images per post*");
    }

    [Fact]
    public void Post_Should_Increment_LikesCount()
    {
        // Arrange
        var post = new Post("post123", "user123", "Test post");
        var initialCount = post.LikesCount;

        // Act
        post.IncrementLikesCount();

        // Assert
        post.LikesCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void Post_Should_Decrement_LikesCount()
    {
        // Arrange
        var post = new Post("post123", "user123", "Test post");
        post.IncrementLikesCount(); // Set to 1 first
        var initialCount = post.LikesCount;

        // Act
        post.DecrementLikesCount();

        // Assert
        post.LikesCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public void Post_Should_Not_Allow_Negative_LikesCount()
    {
        // Arrange
        var post = new Post("post123", "user123", "Test post");

        // Act & Assert
        var action = () => post.DecrementLikesCount();
        
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Likes count cannot be negative");
    }

    [Fact]
    public void Post_Should_Increment_CommentsCount()
    {
        // Arrange
        var post = new Post("post123", "user123", "Test post");
        var initialCount = post.CommentsCount;

        // Act
        post.IncrementCommentsCount();

        // Assert
        post.CommentsCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void Post_Should_Decrement_CommentsCount()
    {
        // Arrange
        var post = new Post("post123", "user123", "Test post");
        post.IncrementCommentsCount(); // Set to 1 first
        var initialCount = post.CommentsCount;

        // Act
        post.DecrementCommentsCount();

        // Assert
        post.CommentsCount.Should().Be(initialCount - 1);
    }
}
