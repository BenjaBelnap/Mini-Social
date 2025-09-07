using MongoDB.Driver;

namespace MiniSocial.Infrastructure.Migrations;

/// <summary>
/// Interface for database migrations.
/// </summary>
public interface IMigration
{
    /// <summary>
    /// The version identifier for this migration (e.g., "001", "002").
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Human-readable description of what this migration does.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Executes the migration forward.
    /// </summary>
    /// <param name="database">The MongoDB database instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpAsync(IMongoDatabase database);
    
    /// <summary>
    /// Executes the migration backward (rollback).
    /// </summary>
    /// <param name="database">The MongoDB database instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DownAsync(IMongoDatabase database);
}
