# Database Migration Guide

This document explains the C# migration system implemented in Mini-Social.

## Overview

The migration system provides:
- **Incremental Updates**: Apply only missing migrations from any database state
- **Version Tracking**: Database tracks applied migrations in a `migrations` collection
- **Type Safety**: C# migrations can reference your domain entities
- **Rollback Support**: Down methods for reversing migrations
- **Command Line Interface**: Simple CLI for migration management

## Migration Commands

### Run All Pending Migrations
```bash
dotnet run --project src/MiniSocial.Api migrate
```
This will:
- Scan for available migrations in the assembly
- Check which migrations have been applied
- Run only the missing migrations in order
- Record successful migrations in the database

### Check Migration Status
```bash
dotnet run --project src/MiniSocial.Api migrate --status
```
Shows:
- Current database version
- List of applied migrations with timestamps
- List of pending migrations

### Migrate to Specific Version
```bash
dotnet run --project src/MiniSocial.Api migrate --target-version 003
```
Applies all migrations up to and including the specified version.

## Creating Migrations

### 1. Create Migration File
Create a new file in `src/MiniSocial.Infrastructure/Migrations/Scripts/`:

```csharp
// Migration003_AddPostTags.cs
using MongoDB.Bson;
using MongoDB.Driver;

namespace MiniSocial.Infrastructure.Migrations.Scripts;

public class Migration003_AddPostTags : IMigration
{
    public string Version => "003";
    public string Description => "Add tagging support to posts";
    
    public async Task UpAsync(IMongoDatabase database)
    {
        // Add your migration logic here
        // Example: Add new collection, update schemas, create indexes
        
        var postsCollection = database.GetCollection<BsonDocument>("posts");
        
        // Add a new index for tags
        try
        {
            await postsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("tags")));
        }
        catch (MongoCommandException ex) when (ex.Code == 85)
        {
            // Index already exists - ignore
        }
    }
    
    public async Task DownAsync(IMongoDatabase database)
    {
        // Reverse the migration
        var postsCollection = database.GetCollection<BsonDocument>("posts");
        
        try
        {
            await postsCollection.Indexes.DropOneAsync("tags_1");
        }
        catch (MongoCommandException)
        {
            // Index doesn't exist - ignore
        }
    }
}
```

### 2. Naming Convention
- **File**: `Migration{XXX}_{Description}.cs`
- **Class**: `Migration{XXX}_{Description}`
- **Version**: Three-digit string (e.g., "001", "002", "003")

### 3. Migration Structure
Each migration must implement `IMigration`:

```csharp
public interface IMigration
{
    string Version { get; }      // "003"
    string Description { get; }  // Human-readable description
    Task UpAsync(IMongoDatabase database);    // Apply migration
    Task DownAsync(IMongoDatabase database);  // Rollback migration
}
```

## Common Migration Patterns

### Adding a New Collection
```csharp
public async Task UpAsync(IMongoDatabase database)
{
    var validationSchema = new BsonDocument
    {
        ["$jsonSchema"] = new BsonDocument
        {
            ["bsonType"] = "object",
            ["required"] = new BsonArray { "_id", "name", "createdAt" },
            ["properties"] = new BsonDocument
            {
                ["_id"] = new BsonDocument { ["bsonType"] = "string" },
                ["name"] = new BsonDocument { ["bsonType"] = "string" },
                ["createdAt"] = new BsonDocument { ["bsonType"] = "date" }
            }
        }
    };
    
    await database.RunCommandAsync<BsonDocument>(new BsonDocument
    {
        ["create"] = "tags",
        ["validator"] = validationSchema
    });
}
```

### Adding Indexes
```csharp
public async Task UpAsync(IMongoDatabase database)
{
    var collection = database.GetCollection<BsonDocument>("posts");
    
    try
    {
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Text("content"),
                new CreateIndexOptions { Name = "content_text_search" }));
    }
    catch (MongoCommandException ex) when (ex.Code == 85)
    {
        // Index already exists - ignore
    }
}
```

### Updating Documents
```csharp
public async Task UpAsync(IMongoDatabase database)
{
    var collection = database.GetCollection<BsonDocument>("users");
    
    // Add a new field to existing documents
    var filter = Builders<BsonDocument>.Filter.Not(
        Builders<BsonDocument>.Filter.Exists("isVerified"));
    var update = Builders<BsonDocument>.Update.Set("isVerified", false);
    
    await collection.UpdateManyAsync(filter, update);
}
```

## Migration Tracking

The system automatically tracks migrations in a `migrations` collection:

```json
{
  "_id": "ObjectId(...)",
  "version": "001",
  "description": "Initial database setup with collections, schemas, and indexes",
  "appliedAt": "2025-09-07T23:28:51.000Z",
  "migrationClass": "Migration001_InitialSetup"
}
```

## Best Practices

### 1. Always Use Try-Catch for Idempotent Operations
```csharp
try
{
    await collection.Indexes.CreateOneAsync(...);
}
catch (MongoCommandException ex) when (ex.Code == 85)
{
    // Index already exists - this is okay
}
```

### 2. Test Migrations Thoroughly
- Test on empty database
- Test on database with existing data
- Test rollback scenarios
- Verify performance impact

### 3. Use Descriptive Names
- Bad: `Migration002_Update.cs`
- Good: `Migration002_AddUserProfilePictures.cs`

### 4. Keep Migrations Small and Focused
Each migration should address one specific change or feature.

### 5. Never Modify Existing Migrations
Once a migration is applied in any environment, treat it as immutable.

## Integration with Application Startup

You can optionally run migrations automatically on application startup:

```csharp
// In Program.cs
var app = builder.Build();

// Run migrations on startup (optional)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    var result = await migrationRunner.MigrateToLatestAsync();
    
    if (!result.Success)
    {
        throw new InvalidOperationException($"Migration failed: {result.ErrorMessage}");
    }
}
```

## Troubleshooting

### Migration Fails
1. Check the error message in the console output
2. Verify database connectivity
3. Ensure the migration logic is correct
4. Check for conflicting schema changes

### Migration History Issues
If you need to manually fix migration history:

```javascript
// Connect to MongoDB and run:
db.migrations.find({}).sort({ version: 1 })  // Check current state
db.migrations.deleteOne({ version: "002" })  // Remove specific migration record
```

### Starting Fresh
To completely reset and re-run all migrations:

```javascript
// ⚠️ WARNING: This will delete all data
db.dropDatabase()
```

Then run migrations again:
```bash
dotnet run migrate
```

## Legacy Database Support

The first migration (`Migration001_InitialSetup`) is designed to work with existing databases created by the old `mongo-init.js` script. It will:

1. **Detect existing collections**: If all expected collections exist, it assumes this is a legacy database
2. **Validate and update**: Ensures indexes exist without recreating collections
3. **Clean slate**: If no collections exist, creates everything from scratch

This allows seamless transition from the old initialization script to the new migration system.
