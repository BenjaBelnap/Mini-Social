using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MiniSocial.Api.Endpoints;
using Xunit;

namespace MiniSocial.Tests.Integration;

public class UserRegistrationIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UserRegistrationIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N"); // Use full GUID for uniqueness
        var registerRequest = new RegisterUserRequest
        {
            Username = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            Password = "SecurePassword123!",
            Bio = "Test user bio"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/register", registerRequest);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        
        response.StatusCode.Should().Be(HttpStatusCode.Created, $"Expected 201 Created but got {response.StatusCode}. Response: {content}");
        
        var userResponse = JsonSerializer.Deserialize<UserResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userResponse.Should().NotBeNull();
        userResponse!.Username.Should().Be($"testuser_{uniqueId}");
        userResponse.Email.Should().Be($"test_{uniqueId}@example.com");
        userResponse.Bio.Should().Be("Test user bio");
        userResponse.Id.Should().NotBeNullOrEmpty();
        userResponse.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var firstUser = new RegisterUserRequest
        {
            Username = $"duplicateuser_{uniqueId}",
            Email = $"first_{uniqueId}@example.com",
            Password = "Password123!",
            Bio = "First user"
        };

        var secondUser = new RegisterUserRequest
        {
            Username = $"duplicateuser_{uniqueId}", // Same username
            Email = $"second_{uniqueId}@example.com",
            Password = "Password123!",
            Bio = "Second user"
        };

        // Act
        await _client.PostAsJsonAsync("/api/users/register", firstUser);
        var response = await _client.PostAsJsonAsync("/api/users/register", secondUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var firstUser = new RegisterUserRequest
        {
            Username = $"user1_{uniqueId}",
            Email = $"duplicate_{uniqueId}@example.com",
            Password = "Password123!",
            Bio = "First user"
        };

        var secondUser = new RegisterUserRequest
        {
            Username = $"user2_{uniqueId}",
            Email = $"duplicate_{uniqueId}@example.com", // Same email
            Password = "Password123!",
            Bio = "Second user"
        };

        // Act
        await _client.PostAsJsonAsync("/api/users/register", firstUser);
        var response = await _client.PostAsJsonAsync("/api/users/register", secondUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterUser_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new RegisterUserRequest
        {
            Username = "", // Empty username
            Email = "test@example.com",
            Password = "password",
            Bio = "Test bio"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/register", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var invalidRequest = new RegisterUserRequest
        {
            Username = $"testuser_{uniqueId}",
            Email = "invalid-email", // Invalid email format
            Password = "Password123!",
            Bio = "Test bio"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/register", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
