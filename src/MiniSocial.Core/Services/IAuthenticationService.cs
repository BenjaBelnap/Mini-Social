using MiniSocial.Core.Entities;

namespace MiniSocial.Core.Services;

public interface IAuthenticationService
{
    Task<User?> AuthenticateAsync(string? email, string? password);
    string HashPassword(string password);
    bool VerifyPassword(string? password, string? hash);
}
