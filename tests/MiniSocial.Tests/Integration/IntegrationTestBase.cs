using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MiniSocial.Tests.Integration;

/// <summary>
/// Base class for integration tests that provides automatic database cleanup.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory<Program>>, IAsyncDisposable
{
    protected readonly TestWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    private readonly IMongoDatabase _database;
    private readonly string _testDatabaseName = "minisocial_test";

    protected IntegrationTestBase(TestWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        // Get database from DI container for cleanup
        using var scope = factory.Services.CreateScope();
        _database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    }

    /// <summary>
    /// Cleans up test data by clearing all documents from collections in the test database.
    /// This ensures each test starts with a clean state while preserving schemas and indexes.
    /// </summary>
    protected virtual async Task CleanupDatabaseAsync()
    {
        try
        {
            var collections = await _database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            foreach (var collectionName in collectionList)
            {
                // Clear documents from collections instead of dropping collections
                var collection = _database.GetCollection<object>(collectionName);
                await collection.DeleteManyAsync(Builders<object>.Filter.Empty);
            }
        }
        catch (Exception)
        {
            // TODO: Replace with proper logging
            // Log the exception but don't fail the test
            // Console.WriteLine($"Warning: Failed to cleanup test database: {ex.Message}");
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        await CleanupDatabaseAsync();
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
