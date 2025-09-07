using MiniSocial.Infrastructure.Migrations;
using MiniSocial.Infrastructure.Extensions;

namespace MiniSocial.Api;

/// <summary>
/// Handles migration commands from the command line.
/// </summary>
public static class MigrationCommands
{
    /// <summary>
    /// Handles migration commands from the command line.
    /// </summary>
    public static async Task<int> RunMigrationCommand(string[] args, WebApplicationBuilder builder)
    {
        // Configure services for migration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddLogging(logging => logging.AddConsole());

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var migrationRunner = serviceProvider.GetRequiredService<IMigrationRunner>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            switch (args.Length)
            {
                case 1 when args[0] == "migrate":
                    // migrate - run all pending migrations
                    Console.WriteLine("Running migrations to latest version...");
                    var result = await migrationRunner.MigrateToLatestAsync();
                    return DisplayMigrationResult(result, logger);

                case 2 when args[0] == "migrate" && args[1] == "--status":
                    // migrate --status - show current migration status
                    await DisplayMigrationStatus(migrationRunner, logger);
                    return 0;

                case 3 when args[0] == "migrate" && args[1] == "--target-version":
                    // migrate --target-version 003 - migrate to specific version
                    var targetVersion = args[2];
                    Console.WriteLine($"Running migrations to version {targetVersion}...");
                    var versionResult = await migrationRunner.MigrateToVersionAsync(targetVersion);
                    return DisplayMigrationResult(versionResult, logger);

                default:
                    Console.WriteLine("Usage:");
                    Console.WriteLine("  dotnet run migrate                           - Run all pending migrations");
                    Console.WriteLine("  dotnet run migrate --status                  - Show migration status");
                    Console.WriteLine("  dotnet run migrate --target-version 003     - Migrate to specific version");
                    return 1;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration command failed");
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Displays the result of a migration operation.
    /// </summary>
    /// <returns>Exit code: 0 for success, 1 for failure</returns>
    private static int DisplayMigrationResult(MigrationResult result, ILogger logger)
    {
        if (result.Success)
        {
            Console.WriteLine($"✅ Migration completed successfully in {result.Duration.TotalMilliseconds:F0}ms");
            
            if (result.AppliedMigrations.Any())
            {
                Console.WriteLine($"Applied migrations ({result.AppliedMigrations.Count}):");
                foreach (var migration in result.AppliedMigrations)
                {
                    Console.WriteLine($"  ✓ {migration}");
                }
            }
            
            if (result.SkippedMigrations.Any())
            {
                Console.WriteLine($"Skipped migrations ({result.SkippedMigrations.Count}):");
                foreach (var migration in result.SkippedMigrations)
                {
                    Console.WriteLine($"  - {migration} (already applied)");
                }
            }
            
            if (!result.AppliedMigrations.Any() && !result.SkippedMigrations.Any())
            {
                Console.WriteLine("No migrations to apply - database is up to date.");
            }
            
            return 0;
        }
        else
        {
            Console.WriteLine($"❌ Migration failed after {result.Duration.TotalMilliseconds:F0}ms");
            Console.WriteLine($"Error: {result.ErrorMessage}");
            
            if (result.AppliedMigrations.Any())
            {
                Console.WriteLine("Migrations applied before failure:");
                foreach (var migration in result.AppliedMigrations)
                {
                    Console.WriteLine($"  ✓ {migration}");
                }
            }
            
            return 1;
        }
    }

    /// <summary>
    /// Displays the current migration status.
    /// </summary>
    private static async Task DisplayMigrationStatus(IMigrationRunner migrationRunner, ILogger logger)
    {
        Console.WriteLine("Migration Status:");
        Console.WriteLine("================");
        
        var currentVersion = await migrationRunner.GetCurrentVersionAsync();
        var appliedMigrations = await migrationRunner.GetAppliedMigrationsAsync();
        var pendingMigrations = await migrationRunner.GetPendingMigrationsAsync();
        
        Console.WriteLine($"Current Version: {currentVersion ?? "None"}");
        Console.WriteLine($"Applied Migrations: {appliedMigrations.Count()}");
        Console.WriteLine($"Pending Migrations: {pendingMigrations.Count()}");
        Console.WriteLine();
        
        if (appliedMigrations.Any())
        {
            Console.WriteLine("Applied Migrations:");
            foreach (var migration in appliedMigrations.OrderBy(m => m.Version))
            {
                Console.WriteLine($"  ✓ {migration.Version} - {migration.Description} (applied {migration.AppliedAt:yyyy-MM-dd HH:mm:ss} UTC)");
            }
            Console.WriteLine();
        }
        
        if (pendingMigrations.Any())
        {
            Console.WriteLine("Pending Migrations:");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"  ⏳ {migration}");
            }
        }
        else
        {
            Console.WriteLine("✅ Database is up to date - no pending migrations.");
        }
    }
}
