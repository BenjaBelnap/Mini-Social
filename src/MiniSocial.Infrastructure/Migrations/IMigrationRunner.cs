using MongoDB.Driver;

namespace MiniSocial.Infrastructure.Migrations;

/// <summary>
/// Interface for running database migrations.
/// </summary>
public interface IMigrationRunner
{
    /// <summary>
    /// Migrates the database to the latest version.
    /// </summary>
    /// <returns>The result of the migration operation.</returns>
    Task<MigrationResult> MigrateToLatestAsync();
    
    /// <summary>
    /// Migrates the database to a specific version.
    /// </summary>
    /// <param name="targetVersion">The target version to migrate to.</param>
    /// <returns>The result of the migration operation.</returns>
    Task<MigrationResult> MigrateToVersionAsync(string targetVersion);
    
    /// <summary>
    /// Gets all migrations that have been applied to the database.
    /// </summary>
    /// <returns>The list of applied migration records.</returns>
    Task<IEnumerable<MigrationRecord>> GetAppliedMigrationsAsync();
    
    /// <summary>
    /// Gets all migrations that are pending (not yet applied).
    /// </summary>
    /// <returns>The list of pending migration versions.</returns>
    Task<IEnumerable<string>> GetPendingMigrationsAsync();
    
    /// <summary>
    /// Gets the current database version (latest applied migration).
    /// </summary>
    /// <returns>The current version, or null if no migrations have been applied.</returns>
    Task<string?> GetCurrentVersionAsync();
}
