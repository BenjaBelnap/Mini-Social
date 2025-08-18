using FluentAssertions;
using MiniSocial.Core.Entities;
using Xunit;

namespace MiniSocial.Tests.Core.Entities;

public class CommentTests
{
    [Fact]
    public void Comment_Should_Be_Created_With_Valid_Properties()
    {
        // Arrange
        var commentId = "comment123";
        var postId = "post123";
        var authorId = "user123";
        var content = "This is a test comment";

        // Act
        var comment = new Comment(commentId, postId, authorId, content);

        // Assert
        comment.Id.Should().Be(commentId);
        comment.PostId.Should().Be(postId);
        comment.AuthorId.Should().Be(authorId);
        comment.Content.Should().Be(content);
        comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        comment.UpdatedAt.Should().BeNull();
        comment.ParentCommentId.Should().BeNull();
        comment.LikesCount.Should().Be(0);
    }

    [Fact]
    public void Comment_Should_Be_Created_As_Reply_With_ParentCommentId()
    {
        // Arrange
        var commentId = "comment123";
        var postId = "post123";
        var authorId = "user123";
        var content = "This is a reply";
        var parentCommentId = "parent456";

        // Act
        var comment = new Comment(commentId, postId, authorId, content, parentCommentId);

        // Assert
        comment.Id.Should().Be(commentId);
        comment.PostId.Should().Be(postId);
        comment.AuthorId.Should().Be(authorId);
        comment.Content.Should().Be(content);
        comment.ParentCommentId.Should().Be(parentCommentId);
        comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Comment_Should_Throw_ArgumentException_When_Content_Is_Invalid(string? invalidContent)
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604 // Possible null reference argument - intentional for testing
        var action = () => new Comment("comment123", "post123", "user123", invalidContent);
#pragma warning restore CS8604
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or whitespace*");
    }

    [Fact]
    public void Comment_Should_Throw_ArgumentException_When_Content_Exceeds_MaxLength()
    {
        // Arrange
        var longContent = new string('a', 1001); // Assuming 1000 character limit for comments

        // Act & Assert
        var action = () => new Comment("comment123", "post123", "user123", longContent);
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot exceed 1000 characters*");
    }

    [Fact]
    public void Comment_Should_Allow_Updating_Content()
    {
        // Arrange
        var comment = new Comment("comment123", "post123", "user123", "Original content");
        var newContent = "Updated content";

        // Act
        comment.UpdateContent(newContent);

        // Assert
        comment.Content.Should().Be(newContent);
        comment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Comment_Should_Increment_LikesCount()
    {
        // Arrange
        var comment = new Comment("comment123", "post123", "user123", "Test comment");
        var initialCount = comment.LikesCount;

        // Act
        comment.IncrementLikesCount();

        // Assert
        comment.LikesCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void Comment_Should_Decrement_LikesCount()
    {
        // Arrange
        var comment = new Comment("comment123", "post123", "user123", "Test comment");
        comment.IncrementLikesCount(); // Set to 1 first
        var initialCount = comment.LikesCount;

        // Act
        comment.DecrementLikesCount();

        // Assert
        comment.LikesCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public void Comment_Should_Not_Allow_Negative_LikesCount()
    {
        // Arrange
        var comment = new Comment("comment123", "post123", "user123", "Test comment");

        // Act & Assert
        var action = () => comment.DecrementLikesCount();
        
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Likes count cannot be negative");
    }
}
