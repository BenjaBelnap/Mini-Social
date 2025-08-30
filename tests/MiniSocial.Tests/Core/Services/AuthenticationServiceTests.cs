using FluentAssertions;
using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;
using MiniSocial.Core.Services;
using Moq;
using Xunit;

namespace MiniSocial.Tests.Core.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _authService = new AuthenticationService(_mockUserRepository.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_User_When_Credentials_Are_Valid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var hashedPassword = _authService.HashPassword(password);
        var user = new User("user123", "testuser", email, hashedPassword);

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_When_User_Not_Found()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password123";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_When_Password_Is_Invalid()
    {
        // Arrange
        var email = "test@example.com";
        var correctPassword = "password123";
        var wrongPassword = "wrongpassword";
        var hashedPassword = _authService.HashPassword(correctPassword);
        var user = new User("user123", "testuser", email, hashedPassword);

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.AuthenticateAsync(email, wrongPassword);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("email@test.com", "")]
    [InlineData(null!, "password")]
    [InlineData("email@test.com", null!)]
    public async Task AuthenticateAsync_Should_Return_Null_When_Credentials_Are_Empty(string? email, string? password)
    {
        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void HashPassword_Should_Return_Different_Hash_For_Same_Password()
    {
        // Arrange
        var password = "testpassword123";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // Each hash should be unique due to random salt
        hash1.Should().NotBeNullOrWhiteSpace();
        hash2.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void HashPassword_Should_Return_Valid_PBKDF2_Format()
    {
        // Arrange
        var password = "testpassword123";

        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        var parts = hash.Split('.', 3);
        parts.Should().HaveCount(3);
        
        // First part should be iterations
        int.Parse(parts[0]).Should().BeGreaterThan(0);
        
        // Second and third parts should be valid base64
        Convert.FromBase64String(parts[1]).Should().HaveCount(32); // 32 byte salt
        Convert.FromBase64String(parts[2]).Should().HaveCount(32); // 32 byte hash
    }

    [Fact]
    public void HashPassword_Should_Throw_When_Password_Is_Null_Or_Empty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _authService.HashPassword(""));
        Assert.Throws<ArgumentException>(() => _authService.HashPassword(null!));
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_When_Password_Matches_Hash()
    {
        // Arrange
        var password = "testpassword123";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_When_Password_Does_Not_Match_Hash()
    {
        // Arrange
        var password = "testpassword123";
        var wrongPassword = "wrongpassword";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("password", "")]
    [InlineData(null!, "hash")]
    [InlineData("password", null!)]
    public void VerifyPassword_Should_Return_False_When_Input_Is_Null_Or_Empty(string? password, string? hash)
    {
        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid.format")]
    [InlineData("notanumber.salt.hash")]
    [InlineData("100000.invalidsalt.hash")]
    [InlineData("100000.dmFsaWRzYWx0.invalidhash")]
    public void VerifyPassword_Should_Return_False_For_Invalid_Hash_Format(string invalidHash)
    {
        // Arrange
        var password = "testpassword123";

        // Act
        var result = _authService.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }
}
