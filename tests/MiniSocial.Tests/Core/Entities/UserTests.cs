using FluentAssertions;
using MiniSocial.Core.Entities;
using Xunit;

namespace MiniSocial.Tests.Core.Entities;

public class UserTests
{
    [Fact]
    public void User_Should_Be_Created_With_Valid_Properties()
    {
        // Arrange
        var userId = "user123";
        var username = "john_doe";
        var email = "john@example.com";
        var passwordHash = "hashedpassword";

        // Act
        var user = new User(userId, username, email, passwordHash);

        // Assert
        user.Id.Should().Be(userId);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.Bio.Should().BeNull();
        user.ProfilePictureUrl.Should().BeNull();
        user.FollowersCount.Should().Be(0);
        user.FollowingCount.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_Should_Throw_ArgumentException_When_Username_Is_Invalid(string invalidUsername)
    {
        // Arrange & Act & Assert
        var action = () => new User("user123", invalidUsername, "john@example.com", "hashedpassword");
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be null or whitespace*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public void User_Should_Throw_ArgumentException_When_Email_Is_Invalid(string invalidEmail)
    {
        // Arrange & Act & Assert
        var action = () => new User("user123", "john_doe", invalidEmail, "hashedpassword");
        
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void User_Should_Allow_Updating_Bio()
    {
        // Arrange
        var user = new User("user123", "john_doe", "john@example.com", "hashedpassword");
        var newBio = "Software developer passionate about clean code";

        // Act
        user.UpdateBio(newBio);

        // Assert
        user.Bio.Should().Be(newBio);
    }

    [Fact]
    public void User_Should_Allow_Updating_Profile_Picture_Url()
    {
        // Arrange
        var user = new User("user123", "john_doe", "john@example.com", "hashedpassword");
        var profilePictureUrl = "https://example.com/profile.jpg";

        // Act
        user.UpdateProfilePicture(profilePictureUrl);

        // Assert
        user.ProfilePictureUrl.Should().Be(profilePictureUrl);
    }

    [Fact]
    public void User_Should_Increment_FollowersCount()
    {
        // Arrange
        var user = new User("user123", "john_doe", "john@example.com", "hashedpassword");
        var initialCount = user.FollowersCount;

        // Act
        user.IncrementFollowersCount();

        // Assert
        user.FollowersCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void User_Should_Decrement_FollowersCount()
    {
        // Arrange
        var user = new User("user123", "john_doe", "john@example.com", "hashedpassword");
        user.IncrementFollowersCount(); // Set to 1 first
        var initialCount = user.FollowersCount;

        // Act
        user.DecrementFollowersCount();

        // Assert
        user.FollowersCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public void User_Should_Not_Allow_Negative_FollowersCount()
    {
        // Arrange
        var user = new User("user123", "john_doe", "john@example.com", "hashedpassword");

        // Act & Assert
        var action = () => user.DecrementFollowersCount();
        
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Followers count cannot be negative");
    }
}
