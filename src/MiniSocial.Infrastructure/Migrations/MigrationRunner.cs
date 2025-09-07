using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MiniSocial.Infrastructure.Migrations;

/// <summary>
/// Service for running database migrations.
/// </summary>
public class MigrationRunner : IMigrationRunner
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MigrationRunner> _logger;
    private readonly IMongoCollection<MigrationRecord> _migrationCollection;
    
    public MigrationRunner(IMongoDatabase database, ILogger<MigrationRunner> logger)
    {
        _database = database;
        _logger = logger;
        _migrationCollection = _database.GetCollection<MigrationRecord>("migrations");
    }
    
    /// <inheritdoc />
    public async Task<MigrationResult> MigrateToLatestAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var appliedMigrations = new List<string>();
        var skippedMigrations = new List<string>();
        
        try
        {
            _logger.LogInformation("Starting migration to latest version");
            
            var availableMigrations = GetAvailableMigrations();
            var appliedMigrationVersions = await GetAppliedMigrationVersionsAsync();
            
            _logger.LogInformation("Found {AvailableCount} available migrations, {AppliedCount} already applied", 
                availableMigrations.Count, appliedMigrationVersions.Count);
            
            foreach (var migration in availableMigrations.OrderBy(m => m.Version))
            {
                if (appliedMigrationVersions.Contains(migration.Version))
                {
                    _logger.LogDebug("Skipping already applied migration {Version}: {Description}", 
                        migration.Version, migration.Description);
                    skippedMigrations.Add(migration.Version);
                    continue;
                }
                
                _logger.LogInformation("Applying migration {Version}: {Description}", 
                    migration.Version, migration.Description);
                
                await migration.UpAsync(_database);
                
                var migrationRecord = new MigrationRecord
                {
                    Version = migration.Version,
                    Description = migration.Description,
                    AppliedAt = DateTime.UtcNow,
                    MigrationClass = migration.GetType().Name
                };
                
                await _migrationCollection.InsertOneAsync(migrationRecord);
                appliedMigrations.Add(migration.Version);
                
                _logger.LogInformation("Successfully applied migration {Version}", migration.Version);
            }
            
            stopwatch.Stop();
            _logger.LogInformation("Migration completed successfully. Applied: {Applied}, Skipped: {Skipped}, Duration: {Duration}ms", 
                appliedMigrations.Count, skippedMigrations.Count, stopwatch.ElapsedMilliseconds);
            
            return MigrationResult.Successful(appliedMigrations, skippedMigrations, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Migration failed after {Duration}ms. Applied migrations: {Applied}", 
                stopwatch.ElapsedMilliseconds, string.Join(", ", appliedMigrations));
            
            return MigrationResult.Failed(ex.Message, appliedMigrations, stopwatch.Elapsed);
        }
    }
    
    /// <inheritdoc />
    public async Task<MigrationResult> MigrateToVersionAsync(string targetVersion)
    {
        var stopwatch = Stopwatch.StartNew();
        var appliedMigrations = new List<string>();
        var skippedMigrations = new List<string>();
        
        try
        {
            _logger.LogInformation("Starting migration to version {TargetVersion}", targetVersion);
            
            var availableMigrations = GetAvailableMigrations();
            var targetMigration = availableMigrations.FirstOrDefault(m => m.Version == targetVersion);
            
            if (targetMigration == null)
            {
                throw new InvalidOperationException($"Migration version {targetVersion} not found");
            }
            
            var appliedMigrationVersions = await GetAppliedMigrationVersionsAsync();
            var migrationsToApply = availableMigrations
                .Where(m => string.Compare(m.Version, targetVersion, StringComparison.Ordinal) <= 0)
                .OrderBy(m => m.Version);
            
            foreach (var migration in migrationsToApply)
            {
                if (appliedMigrationVersions.Contains(migration.Version))
                {
                    skippedMigrations.Add(migration.Version);
                    continue;
                }
                
                _logger.LogInformation("Applying migration {Version}: {Description}", 
                    migration.Version, migration.Description);
                
                await migration.UpAsync(_database);
                
                var migrationRecord = new MigrationRecord
                {
                    Version = migration.Version,
                    Description = migration.Description,
                    AppliedAt = DateTime.UtcNow,
                    MigrationClass = migration.GetType().Name
                };
                
                await _migrationCollection.InsertOneAsync(migrationRecord);
                appliedMigrations.Add(migration.Version);
            }
            
            stopwatch.Stop();
            _logger.LogInformation("Migration to version {TargetVersion} completed successfully", targetVersion);
            
            return MigrationResult.Successful(appliedMigrations, skippedMigrations, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Migration to version {TargetVersion} failed", targetVersion);
            
            return MigrationResult.Failed(ex.Message, appliedMigrations, stopwatch.Elapsed);
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<MigrationRecord>> GetAppliedMigrationsAsync()
    {
        var records = await _migrationCollection
            .Find(Builders<MigrationRecord>.Filter.Empty)
            .SortBy(r => r.Version)
            .ToListAsync();
        
        return records;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetPendingMigrationsAsync()
    {
        var availableMigrations = GetAvailableMigrations();
        var appliedVersions = await GetAppliedMigrationVersionsAsync();
        
        return availableMigrations
            .Where(m => !appliedVersions.Contains(m.Version))
            .OrderBy(m => m.Version)
            .Select(m => m.Version);
    }
    
    /// <inheritdoc />
    public async Task<string?> GetCurrentVersionAsync()
    {
        var latestMigration = await _migrationCollection
            .Find(Builders<MigrationRecord>.Filter.Empty)
            .SortByDescending(r => r.Version)
            .FirstOrDefaultAsync();
        
        return latestMigration?.Version;
    }
    
    private List<IMigration> GetAvailableMigrations()
    {
        var migrations = new List<IMigration>();
        var assembly = Assembly.GetExecutingAssembly();
        
        var migrationTypes = assembly.GetTypes()
            .Where(t => typeof(IMigration).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .OrderBy(t => t.Name);
        
        foreach (var type in migrationTypes)
        {
            if (Activator.CreateInstance(type) is IMigration migration)
            {
                migrations.Add(migration);
            }
        }
        
        return migrations;
    }
    
    private async Task<HashSet<string>> GetAppliedMigrationVersionsAsync()
    {
        var appliedMigrations = await _migrationCollection
            .Find(Builders<MigrationRecord>.Filter.Empty)
            .Project(r => r.Version)
            .ToListAsync();
        
        return new HashSet<string>(appliedMigrations);
    }
}
