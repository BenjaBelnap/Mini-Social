using System.Text.RegularExpressions;

namespace MiniSocial.Core.Entities;

public class User
{
    public string Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? Bio { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int FollowersCount { get; private set; }
    public int FollowingCount { get; private set; }

    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public User(string id, string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or whitespace", nameof(id));
        
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or whitespace", nameof(username));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or whitespace", nameof(email));
        
        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Email format is invalid", nameof(email));
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or whitespace", nameof(passwordHash));

        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        FollowersCount = 0;
        FollowingCount = 0;
    }

    public void UpdateBio(string? bio)
    {
        Bio = bio;
    }

    public void UpdateProfilePicture(string? profilePictureUrl)
    {
        ProfilePictureUrl = profilePictureUrl;
    }

    public void IncrementFollowersCount()
    {
        FollowersCount++;
    }

    public void DecrementFollowersCount()
    {
        if (FollowersCount <= 0)
            throw new InvalidOperationException("Followers count cannot be negative");
        
        FollowersCount--;
    }

    public void IncrementFollowingCount()
    {
        FollowingCount++;
    }

    public void DecrementFollowingCount()
    {
        if (FollowingCount <= 0)
            throw new InvalidOperationException("Following count cannot be negative");
        
        FollowingCount--;
    }
}
