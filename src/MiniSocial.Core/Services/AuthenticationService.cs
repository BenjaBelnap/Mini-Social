using MiniSocial.Core.Entities;
using MiniSocial.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MiniSocial.Core.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    
    // PBKDF2 configuration constants
    private const int SaltSize = 32; // 256 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000; // NIST recommended minimum
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public AuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> AuthenticateAsync(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return null;

        return VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace", nameof(password));

        // Generate salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derive key
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        // Store as: {iterations}.{salt}.{hash}
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string? password, string? storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            // Split stored value
            var parts = storedHash.Split('.', 3);
            if (parts.Length != 3)
            {
                return false;
            }

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

            // Derive key from input password
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithm,
                storedHashBytes.Length);

            // Constant-time comparison
            return CryptographicOperations.FixedTimeEquals(hash, storedHashBytes);
        }
        catch
        {
            // If any parsing or conversion fails, the hash format is invalid
            return false;
        }
    }
}
