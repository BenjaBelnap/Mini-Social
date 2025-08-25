using FluentAssertions;
using MiniSocial.Api.Endpoints;
using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;
using MiniSocial.Core.Services;
using Moq;
using Xunit;

namespace MiniSocial.Tests.Api.Endpoints;

public class UserRegistrationTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IAuthenticationService> _mockAuthService;

    public UserRegistrationTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAuthService = new Mock<IAuthenticationService>();
    }

    [Fact]
    public void RegisterUserRequest_Should_Have_Required_Properties()
    {
        // Arrange & Act
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123",
            Bio = "Test bio"
        };

        // Assert
        request.Username.Should().Be("testuser");
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("password123");
        request.Bio.Should().Be("Test bio");
    }

    [Fact]
    public void UserResponse_Should_Exclude_Sensitive_Information()
    {
        // Arrange & Act
        var response = new UserResponse(
            "user123",
            "testuser",
            "test@example.com",
            "Test bio",
            DateTime.UtcNow
        );

        // Assert
        response.Id.Should().Be("user123");
        response.Username.Should().Be("testuser");
        response.Email.Should().Be("test@example.com");
        response.Bio.Should().Be("Test bio");
        response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        // Verify no password hash is exposed
        var responseType = typeof(UserResponse);
        var properties = responseType.GetProperties();
        properties.Should().NotContain(p => p.Name.Contains("Password"));
        properties.Should().NotContain(p => p.Name.Contains("Hash"));
    }
}
