using FluentAssertions;
using MiniSocial.Core.Entities;
using Xunit;

namespace MiniSocial.Tests.Core.Entities;

public class FollowTests
{
    [Fact]
    public void Follow_Should_Be_Created_With_Valid_Properties()
    {
        // Arrange
        var followId = "follow123";
        var followerId = "user123";
        var followeeId = "user456";

        // Act
        var follow = new Follow(followId, followerId, followeeId);

        // Assert
        follow.Id.Should().Be(followId);
        follow.FollowerId.Should().Be(followerId);
        follow.FolloweeId.Should().Be(followeeId);
        follow.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Follow_Should_Throw_ArgumentException_When_FollowerId_Is_Invalid(string? invalidFollowerId)
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604 // Possible null reference argument - intentional for testing
        var action = () => new Follow("follow123", invalidFollowerId, "user456");
#pragma warning restore CS8604
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Follower ID cannot be null or whitespace*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Follow_Should_Throw_ArgumentException_When_FolloweeId_Is_Invalid(string? invalidFolloweeId)
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604 // Possible null reference argument - intentional for testing
        var action = () => new Follow("follow123", "user123", invalidFolloweeId);
#pragma warning restore CS8604
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Followee ID cannot be null or whitespace*");
    }

    [Fact]
    public void Follow_Should_Throw_ArgumentException_When_User_Tries_To_Follow_Themselves()
    {
        // Arrange
        var userId = "user123";

        // Act & Assert
        var action = () => new Follow("follow123", userId, userId);
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("User cannot follow themselves*");
    }
}
