using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;
using MiniSocial.Core.Services;

namespace MiniSocial.Api.Endpoints;

/// <summary>
/// Endpoints for user registration and management
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Maps user endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    public static void MapUserEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        users.MapPost("/register", RegisterUser)
            .WithSummary("Register new user")
            .WithDescription("Creates a new user account with the provided information")
            .Accepts<RegisterUserRequest>("application/json")
            .Produces<UserResponse>(201)
            .ProducesValidationProblem()
            .ProducesProblem(500);
    }

    /// <summary>
    /// Creates a new user account
    /// </summary>
    /// <param name="request">The user registration request containing user details</param>
    /// <param name="userRepository">The user repository service</param>
    /// <param name="authService">The authentication service for password hashing</param>
    /// <param name="logger">The logger for capturing errors</param>
    /// <param name="environment">The web host environment for determining error detail level</param>
    /// <returns>The newly created user</returns>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the request data is invalid or user already exists</response>
    /// <response code="500">If there was an internal server error</response>
    private static async Task<IResult> RegisterUser(
        RegisterUserRequest request,
        IUserRepository userRepository,
        IAuthenticationService authService,
        ILogger<object> logger,
        IWebHostEnvironment environment)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest("Username, email, and password are required");
            }

            // Check if username is available
            if (!await userRepository.IsUsernameAvailableAsync(request.Username))
            {
                return Results.BadRequest("Username is already taken");
            }

            // Check if email is available
            if (!await userRepository.IsEmailAvailableAsync(request.Email))
            {
                return Results.BadRequest("Email is already registered");
            }

            // Hash the password
            var hashedPassword = authService.HashPassword(request.Password);

            // Create the user
            var user = new User(
                Guid.NewGuid().ToString(),
                request.Username,
                request.Email,
                hashedPassword
            );

            // Update bio if provided
            if (!string.IsNullOrWhiteSpace(request.Bio))
            {
                user.UpdateBio(request.Bio);
            }

            // Debug log the user object
            logger.LogInformation("Creating user: Id={Id}, Username={Username}, Email={Email}, CreatedAt={CreatedAt}", 
                user.Id, user.Username, user.Email, user.CreatedAt);

            // Save to repository
            try
            {
                var createdUser = await userRepository.CreateAsync(user);
                logger.LogInformation("Successfully created user with Id={Id}", createdUser.Id);
                
                // Return response without password hash
                var response = new UserResponse(
                    createdUser.Id,
                    createdUser.Username,
                    createdUser.Email,
                    createdUser.Bio,
                    createdUser.CreatedAt
                );

                return Results.Created($"/api/users/{createdUser.Id}", response);
            }
            catch (Exception repositoryEx)
            {
                logger.LogError(repositoryEx, "Repository operation failed for user with Id={Id}, Username={Username}", 
                    user.Id, user.Username);
                throw; // Re-throw to be caught by outer exception handler
            }
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid user registration data: {Message}", ex.Message);
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating user with username: {Username}", request.Username);
            
            // In development or test environments, expose more error details
            if (environment.IsDevelopment() || environment.EnvironmentName == "Test")
            {
                return Results.Problem(
                    title: "An error occurred while creating the user",
                    detail: $"Exception: {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}",
                    statusCode: 500
                );
            }
            
            return Results.Problem("An error occurred while creating the user");
        }
    }
}

/// <summary>
/// Request model for user registration
/// </summary>
public record RegisterUserRequest
{
    /// <summary>
    /// The unique username for the account
    /// </summary>
    /// <example>john_doe</example>
    public required string Username { get; init; }
    
    /// <summary>
    /// The user's email address
    /// </summary>
    /// <example>john@example.com</example>
    public required string Email { get; init; }
    
    /// <summary>
    /// The user's password (will be hashed)
    /// </summary>
    /// <example>SecurePassword123!</example>
    public required string Password { get; init; }
    
    /// <summary>
    /// Optional bio for the user
    /// </summary>
    /// <example>Software developer who loves coding</example>
    public string? Bio { get; init; }
}

/// <summary>
/// Response model for user data (excludes sensitive information)
/// </summary>
public record UserResponse(
    string Id,
    string Username,
    string Email,
    string? Bio,
    DateTime CreatedAt
);
