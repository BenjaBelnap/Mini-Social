using FluentAssertions;
using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;
using Moq;
using Xunit;

namespace MiniSocial.Tests.Core.Interfaces;

public class UserRepositoryInterfaceTests
{
    [Fact]
    public async Task IUserRepository_Should_Define_Required_Methods()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var user = new User("user123", "john_doe", "john@example.com", "hashedpassword");
        
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);
        mockRepo.Setup(r => r.GetByIdAsync("user123"))
            .ReturnsAsync(user);
        mockRepo.Setup(r => r.GetByUsernameAsync("john_doe"))
            .ReturnsAsync(user);
        mockRepo.Setup(r => r.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);
        mockRepo.Setup(r => r.IsUsernameAvailableAsync("john_doe"))
            .ReturnsAsync(false);
        mockRepo.Setup(r => r.IsEmailAvailableAsync("john@example.com"))
            .ReturnsAsync(false);

        // Act & Assert
        var createdUser = await mockRepo.Object.CreateAsync(user);
        createdUser.Should().Be(user);

        var retrievedById = await mockRepo.Object.GetByIdAsync("user123");
        retrievedById.Should().Be(user);

        var retrievedByUsername = await mockRepo.Object.GetByUsernameAsync("john_doe");
        retrievedByUsername.Should().Be(user);

        var retrievedByEmail = await mockRepo.Object.GetByEmailAsync("john@example.com");
        retrievedByEmail.Should().Be(user);

        var usernameAvailable = await mockRepo.Object.IsUsernameAvailableAsync("john_doe");
        usernameAvailable.Should().BeFalse();

        var emailAvailable = await mockRepo.Object.IsEmailAvailableAsync("john@example.com");
        emailAvailable.Should().BeFalse();
        
        // Verify all methods were called
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        mockRepo.Verify(r => r.GetByIdAsync("user123"), Times.Once);
        mockRepo.Verify(r => r.GetByUsernameAsync("john_doe"), Times.Once);
        mockRepo.Verify(r => r.GetByEmailAsync("john@example.com"), Times.Once);
        mockRepo.Verify(r => r.IsUsernameAvailableAsync("john_doe"), Times.Once);
        mockRepo.Verify(r => r.IsEmailAvailableAsync("john@example.com"), Times.Once);
    }
}
