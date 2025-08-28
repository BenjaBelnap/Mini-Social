using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization;
using MiniSocial.Core.Entities;

namespace MiniSocial.Tests.Infrastructure;

/// <summary>
/// Base class for tests that interact with MongoDB.
/// Provides automatic database cleanup after each test.
/// </summary>
public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected IMongoDatabase Database { get; private set; } = null!;
    protected IMongoClient Client { get; private set; } = null!;
    private readonly string _testDatabaseName = "minisocial_test";
    
    protected DatabaseTestBase()
    {
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        // Configure MongoDB serialization
        ConfigureMongoDbSerialization();
        
        // Load environment variables from database/.env file
        var envFilePath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", "..", 
            "database", ".env");
            
        Console.WriteLine($"Looking for .env file at: {envFilePath}");
        Console.WriteLine($"File exists: {File.Exists(envFilePath)}");
            
        if (File.Exists(envFilePath))
        {
            DotNetEnv.Env.Load(envFilePath);
            Console.WriteLine("Loaded .env file successfully");
        }
        else
        {
            Console.WriteLine("Warning: .env file not found, using default values");
        }
        
        var connectionString = BuildTestConnectionString();
        Console.WriteLine($"Using connection string: {connectionString}");
        Client = new MongoClient(connectionString);
        Database = Client.GetDatabase(_testDatabaseName);
    }
    
    private static string BuildTestConnectionString()
    {
        var username = Environment.GetEnvironmentVariable("MONGO_TEST_USERNAME") ?? "minisocial_test_user";
        var password = Environment.GetEnvironmentVariable("MONGO_TEST_PASSWORD") ?? "minisocial_test_password";
        var host = Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("MONGO_PORT") ?? "27017";
        var database = Environment.GetEnvironmentVariable("MONGO_TEST_DATABASE") ?? "minisocial_test";
        
        return $"mongodb://{username}:{password}@{host}:{port}/{database}?authSource={database}";
    }
    
    private static void ConfigureMongoDbSerialization()
    {
        // Only register conventions and class maps if not already registered
        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            // Configure camelCase naming convention for MongoDB field names
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("camelCase", conventionPack, t => true);
        
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                // Map the Id property to MongoDB's _id field
                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.Username).SetElementName("username");
                cm.MapMember(c => c.Email).SetElementName("email");
                cm.MapMember(c => c.PasswordHash).SetElementName("passwordHash");
                cm.MapMember(c => c.Bio).SetElementName("bio").SetIgnoreIfNull(true);
                cm.MapMember(c => c.ProfilePictureUrl).SetElementName("profilePictureUrl").SetIgnoreIfNull(true);
                cm.MapMember(c => c.CreatedAt).SetElementName("createdAt");
                cm.MapMember(c => c.FollowersCount).SetElementName("followersCount");
                cm.MapMember(c => c.FollowingCount).SetElementName("followingCount");
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
    
    /// <summary>
    /// Cleans up test data by dropping all collections in the test database.
    /// This is faster than dropping the entire database and recreating it.
    /// </summary>
    protected virtual async Task CleanupDatabaseAsync()
    {
        try
        {
            var collections = await Database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            foreach (var collectionName in collectionList)
            {
                // Clear documents from collections instead of dropping collections
                var collection = Database.GetCollection<object>(collectionName);
                await collection.DeleteManyAsync(Builders<object>.Filter.Empty);
            }
        }
        catch (Exception ex)
        {
            // Log the exception but don't fail the test
            Console.WriteLine($"Warning: Failed to cleanup test database: {ex.Message}");
        }
    }
    
    public virtual async ValueTask DisposeAsync()
    {
        await CleanupDatabaseAsync();
        GC.SuppressFinalize(this);
    }
    
    // IAsyncLifetime implementation
    public virtual Task InitializeAsync()
    {
        // Initialization is done in constructor, nothing additional needed
        return Task.CompletedTask;
    }
    
    async Task IAsyncLifetime.DisposeAsync()
    {
        await CleanupDatabaseAsync();
    }
}
