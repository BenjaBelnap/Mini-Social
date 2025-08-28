using FluentAssertions;
using MiniSocial.Core.Entities;
using MiniSocial.Infrastructure.Repositories;
using MiniSocial.Infrastructure.Configuration;
using MongoDB.Driver;
using Xunit;

namespace MiniSocial.Tests.Infrastructure.Repositories;

[Trait("Category", "Integration")]
public class UserRepositoryTests : DatabaseTestBase
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = new UserRepository(Database);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_User_Successfully()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N");
        var user = new User($"user_{testId}", $"john_doe_{testId}", $"john_{testId}@example.com", "hashedpassword");

        // Act
        var createdUser = await _repository.CreateAsync(user);

        // Assert
        createdUser.Should().NotBeNull();
        createdUser.Id.Should().Be(user.Id);
        createdUser.Username.Should().Be(user.Username);
        createdUser.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User_When_Exists()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N");
        var user = new User($"user_{testId}", $"john_doe_{testId}", $"john_{testId}@example.com", "hashedpassword");
        await _repository.CreateAsync(user);

        // Act
        var retrievedUser = await _repository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(user.Id);
        retrievedUser.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Act
        var retrievedUser = await _repository.GetByIdAsync("nonexistent");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_Return_User_When_Exists()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N");
        var user = new User($"user_{testId}", $"john_doe_{testId}", $"john_{testId}@example.com", "hashedpassword");
        await _repository.CreateAsync(user);

        // Act
        var retrievedUser = await _repository.GetByUsernameAsync($"john_doe_{testId}");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Username.Should().Be($"john_doe_{testId}");
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_User_When_Exists()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N");
        var user = new User($"user_{testId}", $"john_doe_{testId}", $"john_{testId}@example.com", "hashedpassword");
        await _repository.CreateAsync(user);

        // Act
        var retrievedUser = await _repository.GetByEmailAsync($"john_{testId}@example.com");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be($"john_{testId}@example.com");
    }

    [Fact]
    public async Task IsUsernameAvailableAsync_Should_Return_False_When_Username_Taken()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N");
        var user = new User($"user_{testId}", $"john_doe_{testId}", $"john_{testId}@example.com", "hashedpassword");
        await _repository.CreateAsync(user);

        // Act
        var isAvailable = await _repository.IsUsernameAvailableAsync($"john_doe_{testId}");

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task IsUsernameAvailableAsync_Should_Return_True_When_Username_Available()
    {
        // Act
        var isAvailable = await _repository.IsUsernameAvailableAsync("available_username");

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_User_Successfully()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N");
        var user = new User($"user_{testId}", $"john_doe_{testId}", $"john_{testId}@example.com", "hashedpassword");
        await _repository.CreateAsync(user);
        
        user.UpdateBio("Updated bio");

        // Act
        var updatedUser = await _repository.UpdateAsync(user);

        // Assert
        updatedUser.Should().NotBeNull();
        updatedUser.Bio.Should().Be("Updated bio");
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_User_Successfully()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N");
        var user = new User($"user_{testId}", $"john_doe_{testId}", $"john_{testId}@example.com", "hashedpassword");
        await _repository.CreateAsync(user);

        // Act
        var deleted = await _repository.DeleteAsync(user.Id);

        // Assert
        deleted.Should().BeTrue();
        
        var retrievedUser = await _repository.GetByIdAsync(user.Id);
        retrievedUser.Should().BeNull();
    }
}
