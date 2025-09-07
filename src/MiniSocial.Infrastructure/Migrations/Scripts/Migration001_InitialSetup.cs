using MongoDB.Bson;
using MongoDB.Driver;

namespace MiniSocial.Infrastructure.Migrations.Scripts;

/// <summary>
/// Initial database setup migration.
/// Creates collections with validation schemas and indexes.
/// </summary>
public class Migration001_InitialSetup : IMigration
{
    public string Version => "001";
    public string Description => "Initial database setup with collections, schemas, and indexes";
    
    public async Task UpAsync(IMongoDatabase database)
    {
        // Check if migration has already been applied by checking for the migration signature
        var collections = await database.ListCollectionNamesAsync();
        var collectionList = await collections.ToListAsync();
        
        // If collections already exist, this might be a legacy database
        // We'll validate that the expected collections exist and skip creation
        var expectedCollections = new[] { "users", "posts", "comments", "follows" };
        var existingCollections = expectedCollections.Where(c => collectionList.Contains(c)).ToList();
        
        if (existingCollections.Count == expectedCollections.Length)
        {
            // All collections exist - validate they have the expected structure and indexes
            await ValidateAndUpdateExistingCollectionsAsync(database);
        }
        else if (existingCollections.Count == 0)
        {
            // Clean slate - create everything new
            await CreateNewCollectionsAsync(database);
        }
        else
        {
            // Partial state - this shouldn't happen in normal circumstances
            throw new InvalidOperationException(
                $"Database is in an unexpected state. Found collections: {string.Join(", ", existingCollections)}. " +
                "Expected either all collections to exist or none to exist.");
        }
    }
    
    private async Task CreateNewCollectionsAsync(IMongoDatabase database)
    {
        // Create users collection with validation schema
        await CreateUsersCollectionAsync(database);
        
        // Create posts collection with validation schema
        await CreatePostsCollectionAsync(database);
        
        // Create comments collection with validation schema
        await CreateCommentsCollectionAsync(database);
        
        // Create follows collection with validation schema
        await CreateFollowsCollectionAsync(database);
        
        // Create indexes for better performance
        await CreateIndexesAsync(database);
    }
    
    private async Task ValidateAndUpdateExistingCollectionsAsync(IMongoDatabase database)
    {
        // For existing collections, we'll ensure indexes exist
        // Schema validation updates would require more complex migration logic
        await CreateIndexesAsync(database);
    }
    
    public async Task DownAsync(IMongoDatabase database)
    {
        // Drop all collections created in this migration
        await database.DropCollectionAsync("users");
        await database.DropCollectionAsync("posts");
        await database.DropCollectionAsync("comments");
        await database.DropCollectionAsync("follows");
    }
    
    private async Task CreateUsersCollectionAsync(IMongoDatabase database)
    {
        var validationSchema = new BsonDocument
        {
            ["$jsonSchema"] = new BsonDocument
            {
                ["bsonType"] = "object",
                ["required"] = new BsonArray { "_id", "username", "email", "passwordHash", "createdAt" },
                ["properties"] = new BsonDocument
                {
                    ["_id"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["username"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["minLength"] = 3,
                        ["maxLength"] = 50,
                        ["description"] = "must be a string between 3-50 characters and is required"
                    },
                    ["email"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["pattern"] = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
                        ["description"] = "must be a valid email address and is required"
                    },
                    ["passwordHash"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["firstName"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["maxLength"] = 100,
                        ["description"] = "must be a string with max length 100"
                    },
                    ["lastName"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["maxLength"] = 100,
                        ["description"] = "must be a string with max length 100"
                    },
                    ["bio"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["maxLength"] = 500,
                        ["description"] = "must be a string with max length 500"
                    },
                    ["profilePictureUrl"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string"
                    },
                    ["isEmailVerified"] = new BsonDocument
                    {
                        ["bsonType"] = "bool",
                        ["description"] = "must be a boolean"
                    },
                    ["isActive"] = new BsonDocument
                    {
                        ["bsonType"] = "bool",
                        ["description"] = "must be a boolean"
                    },
                    ["followersCount"] = new BsonDocument
                    {
                        ["bsonType"] = "int",
                        ["minimum"] = 0,
                        ["description"] = "must be a non-negative integer"
                    },
                    ["followingCount"] = new BsonDocument
                    {
                        ["bsonType"] = "int",
                        ["minimum"] = 0,
                        ["description"] = "must be a non-negative integer"
                    },
                    ["createdAt"] = new BsonDocument
                    {
                        ["bsonType"] = "date",
                        ["description"] = "must be a date and is required"
                    },
                    ["updatedAt"] = new BsonDocument
                    {
                        ["bsonType"] = "date",
                        ["description"] = "must be a date"
                    }
                }
            }
        };
        
        // Create users collection with validation schema
        await database.RunCommandAsync<BsonDocument>(new BsonDocument
        {
            ["create"] = "users",
            ["validator"] = validationSchema
        });
    }
    
    private async Task CreatePostsCollectionAsync(IMongoDatabase database)
    {
        var validationSchema = new BsonDocument
        {
            ["$jsonSchema"] = new BsonDocument
            {
                ["bsonType"] = "object",
                ["required"] = new BsonArray { "_id", "userId", "content", "createdAt" },
                ["properties"] = new BsonDocument
                {
                    ["_id"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["userId"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["content"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["minLength"] = 1,
                        ["maxLength"] = 1000,
                        ["description"] = "must be a string between 1-1000 characters and is required"
                    },
                    ["imageUrls"] = new BsonDocument
                    {
                        ["bsonType"] = "array",
                        ["items"] = new BsonDocument
                        {
                            ["bsonType"] = "string"
                        },
                        ["description"] = "must be an array of strings"
                    },
                    ["likes"] = new BsonDocument
                    {
                        ["bsonType"] = "int",
                        ["minimum"] = 0,
                        ["description"] = "must be a non-negative integer"
                    },
                    ["isEdited"] = new BsonDocument
                    {
                        ["bsonType"] = "bool",
                        ["description"] = "must be a boolean"
                    },
                    ["createdAt"] = new BsonDocument
                    {
                        ["bsonType"] = "date",
                        ["description"] = "must be a date and is required"
                    },
                    ["updatedAt"] = new BsonDocument
                    {
                        ["bsonType"] = "date",
                        ["description"] = "must be a date"
                    }
                }
            }
        };
        
        // Create posts collection with validation schema
        await database.RunCommandAsync<BsonDocument>(new BsonDocument
        {
            ["create"] = "posts",
            ["validator"] = validationSchema
        });
    }
    
    private async Task CreateCommentsCollectionAsync(IMongoDatabase database)
    {
        var validationSchema = new BsonDocument
        {
            ["$jsonSchema"] = new BsonDocument
            {
                ["bsonType"] = "object",
                ["required"] = new BsonArray { "_id", "postId", "userId", "content", "createdAt" },
                ["properties"] = new BsonDocument
                {
                    ["_id"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["postId"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["userId"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["content"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["minLength"] = 1,
                        ["maxLength"] = 500,
                        ["description"] = "must be a string between 1-500 characters and is required"
                    },
                    ["likes"] = new BsonDocument
                    {
                        ["bsonType"] = "int",
                        ["minimum"] = 0,
                        ["description"] = "must be a non-negative integer"
                    },
                    ["isEdited"] = new BsonDocument
                    {
                        ["bsonType"] = "bool",
                        ["description"] = "must be a boolean"
                    },
                    ["createdAt"] = new BsonDocument
                    {
                        ["bsonType"] = "date",
                        ["description"] = "must be a date and is required"
                    },
                    ["updatedAt"] = new BsonDocument
                    {
                        ["bsonType"] = "date",
                        ["description"] = "must be a date"
                    }
                }
            }
        };
        
        // Create comments collection with validation schema
        await database.RunCommandAsync<BsonDocument>(new BsonDocument
        {
            ["create"] = "comments",
            ["validator"] = validationSchema
        });
    }
    
    private async Task CreateFollowsCollectionAsync(IMongoDatabase database)
    {
        var validationSchema = new BsonDocument
        {
            ["$jsonSchema"] = new BsonDocument
            {
                ["bsonType"] = "object",
                ["required"] = new BsonArray { "_id", "followerId", "followeeId", "createdAt" },
                ["properties"] = new BsonDocument
                {
                    ["_id"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["followerId"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["followeeId"] = new BsonDocument
                    {
                        ["bsonType"] = "string",
                        ["description"] = "must be a string and is required"
                    },
                    ["createdAt"] = new BsonDocument
                    {
                        ["bsonType"] = "date",
                        ["description"] = "must be a date and is required"
                    }
                }
            }
        };
        
        // Create follows collection with validation schema
        await database.RunCommandAsync<BsonDocument>(new BsonDocument
        {
            ["create"] = "follows",
            ["validator"] = validationSchema
        });
    }
    
    private async Task CreateIndexesAsync(IMongoDatabase database)
    {
        // Users indexes
        var usersCollection = database.GetCollection<BsonDocument>("users");
        await CreateIndexSafelyAsync(usersCollection, 
            Builders<BsonDocument>.IndexKeys.Ascending("username"),
            new CreateIndexOptions { Unique = true });
        
        await CreateIndexSafelyAsync(usersCollection,
            Builders<BsonDocument>.IndexKeys.Ascending("email"),
            new CreateIndexOptions { Unique = true });
        
        // Posts indexes
        var postsCollection = database.GetCollection<BsonDocument>("posts");
        await CreateIndexSafelyAsync(postsCollection,
            Builders<BsonDocument>.IndexKeys.Ascending("userId"));
        
        await CreateIndexSafelyAsync(postsCollection,
            Builders<BsonDocument>.IndexKeys.Descending("createdAt"));
        
        // Comments indexes
        var commentsCollection = database.GetCollection<BsonDocument>("comments");
        await CreateIndexSafelyAsync(commentsCollection,
            Builders<BsonDocument>.IndexKeys.Ascending("postId"));
        
        await CreateIndexSafelyAsync(commentsCollection,
            Builders<BsonDocument>.IndexKeys.Ascending("userId"));
        
        // Follows indexes
        var followsCollection = database.GetCollection<BsonDocument>("follows");
        await CreateIndexSafelyAsync(followsCollection,
            Builders<BsonDocument>.IndexKeys.Ascending("followerId").Ascending("followeeId"),
            new CreateIndexOptions { Unique = true });
        
        await CreateIndexSafelyAsync(followsCollection,
            Builders<BsonDocument>.IndexKeys.Ascending("followerId"));
        
        await CreateIndexSafelyAsync(followsCollection,
            Builders<BsonDocument>.IndexKeys.Ascending("followeeId"));
    }
    
    /// <summary>
    /// Creates an index safely, ignoring errors if the index already exists.
    /// </summary>
    /// <param name="collection">The MongoDB collection.</param>
    /// <param name="indexKeys">The index key specification.</param>
    /// <param name="options">Optional index creation options.</param>
    private static async Task CreateIndexSafelyAsync(IMongoCollection<BsonDocument> collection, 
        IndexKeysDefinition<BsonDocument> indexKeys, 
        CreateIndexOptions? options = null)
    {
        try
        {
            var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, options);
            await collection.Indexes.CreateOneAsync(indexModel);
        }
        catch (MongoCommandException ex) when (ex.Code == 85)
        {
            // Index already exists - ignore
        }
    }
}
