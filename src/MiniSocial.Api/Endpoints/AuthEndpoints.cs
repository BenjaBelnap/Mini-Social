using MiniSocial.Core.Services;

namespace MiniSocial.Api.Endpoints;

/// <summary>
/// Endpoints for user authentication
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        auth.MapPost("/login", Login)
            .WithSummary("User login")
            .WithDescription("Authenticates a user with email and password")
            .Accepts<LoginRequest>("application/json")
            .Produces<LoginResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(400);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IAuthenticationService authService)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest("Email and password are required");
        }

        var user = await authService.AuthenticateAsync(request.Email, request.Password);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        // For now, return basic user info. Later we'll add JWT token here
        var response = new LoginResponse(
            user.Id,
            user.Username,
            user.Email,
            "TODO: Add JWT token here"
        );

        return Results.Ok(response);
    }
}

/// <summary>
/// Request model for user login
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's password</param>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Response model for successful login
/// </summary>
/// <param name="UserId">The user's unique identifier</param>
/// <param name="Username">The user's username</param>
/// <param name="Email">The user's email address</param>
/// <param name="Token">Authentication token (will be JWT token later)</param>
public record LoginResponse(string UserId, string Username, string Email, string Token);
