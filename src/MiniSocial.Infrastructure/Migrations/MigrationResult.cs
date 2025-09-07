namespace MiniSocial.Infrastructure.Migrations;

/// <summary>
/// Result of a migration operation.
/// </summary>
public class MigrationResult
{
    /// <summary>
    /// Indicates whether the migration operation was successful.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if the migration failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// List of migrations that were applied.
    /// </summary>
    public List<string> AppliedMigrations { get; set; } = new();
    
    /// <summary>
    /// List of migrations that were skipped (already applied).
    /// </summary>
    public List<string> SkippedMigrations { get; set; } = new();
    
    /// <summary>
    /// Total time taken for the migration operation.
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Creates a successful migration result.
    /// </summary>
    public static MigrationResult Successful(List<string> appliedMigrations, List<string> skippedMigrations, TimeSpan duration)
    {
        return new MigrationResult
        {
            Success = true,
            AppliedMigrations = appliedMigrations,
            SkippedMigrations = skippedMigrations,
            Duration = duration
        };
    }
    
    /// <summary>
    /// Creates a failed migration result.
    /// </summary>
    public static MigrationResult Failed(string errorMessage, List<string> appliedMigrations, TimeSpan duration)
    {
        return new MigrationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            AppliedMigrations = appliedMigrations,
            Duration = duration
        };
    }
}
