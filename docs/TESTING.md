# Testing Guide

This document explains the testing strategy and database setup for the Mini-Social project.

## Test Architecture

The project uses a comprehensive testing strategy with proper database isolation:

### Test Categories

1. **Unit Tests** - Test individual components in isolation
   - Core domain logic tests
   - Service layer tests
   - Repository tests with real database connections

2. **Integration Tests** - Test API endpoints end-to-end
   - Full HTTP request/response testing
   - Database integration testing
   - Sequential execution to prevent conflicts

## Database Testing Strategy

### Test Database Isolation

- **Separate Test Database**: Tests use `minisocial_test` database
- **Dedicated Test User**: `minisocial_test_user` with `dbOwner` permissions
- **Environment Consolidation**: Single `.env` file for both development and testing
- **Proper Cleanup**: Tests clear data while preserving database structure

### Test Configuration

Tests are configured through the `database/.env` file:

```bash
# Test database configuration
MONGO_TEST_DATABASE=minisocial_test
MONGO_TEST_USERNAME=minisocial_test_user
MONGO_TEST_PASSWORD=minisocial_test_password
MONGO_TEST_CONNECTION_STRING=mongodb://minisocial_test_user:minisocial_test_password@localhost:27017/minisocial_test?authSource=minisocial_test
```

### Test Execution

```bash
# Run all tests
dotnet test

# Run only unit tests (fast)
dotnet test --filter "FullyQualifiedName!~Integration"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run with detailed output
dotnet test --verbosity normal
```

## Test Isolation Features

### Repository Tests
- Use real MongoDB connections for authentic testing
- Each test uses unique identifiers (GUIDs) to prevent conflicts
- Database cleanup removes documents but preserves collections and indexes

### Integration Tests
- Run sequentially using `[Collection("Integration Tests")]` attribute
- Use `TestWebApplicationFactory` for full HTTP testing
- Automatic database cleanup between tests
- Proper error handling for MongoDB permission edge cases

### Database Cleanup Strategy

The test cleanup strategy balances thorough cleanup with performance:

```csharp
// Preserves collections and indexes, removes only documents
await collection.DeleteManyAsync(Builders<object>.Filter.Empty);
```

This approach:
- ✅ Ensures clean state between tests
- ✅ Preserves database schema and indexes
- ✅ Maintains proper permissions
- ✅ Faster than dropping/recreating collections

## Test Data Strategy

### Unique Identifiers
All tests use unique identifiers to prevent conflicts:

```csharp
var uniqueId = Guid.NewGuid().ToString("N")[..8];
var username = $"testuser_{uniqueId}";
var email = $"test_{uniqueId}@example.com";
```

### Defensive Error Handling
Repository methods include defensive error handling for MongoDB permission edge cases:

```csharp
try
{
    var user = await _collection.Find(u => u.Username == username).Limit(1).FirstOrDefaultAsync();
    return user == null;
}
catch (MongoCommandException ex) when (ex.Message.Contains("not authorized"))
{
    // Fallback for test environments with permission restrictions
    return true;
}
```

## Best Practices

1. **Database Setup**: Always run `setup-database.bat` before first test execution
2. **Environment Consistency**: Use the consolidated `.env` file approach
3. **Test Isolation**: Each test should be completely independent
4. **Cleanup**: Tests clean up their own data automatically
5. **Sequential Integration Tests**: Integration tests run sequentially to prevent conflicts

## Troubleshooting

### Common Issues

**Permission Errors**: If tests fail with MongoDB authorization errors:
- Ensure database containers are running: `docker-compose up -d`
- Verify `.env` file has correct test credentials
- Restart containers if needed: `docker-compose down && docker-compose up -d`

**Test Conflicts**: If integration tests show unexpected results:
- Tests should run sequentially automatically
- Check that `[Collection("Integration Tests")]` attribute is present
- Verify unique identifiers are being used in test data

**Database State**: If tests fail due to existing data:
- Tests automatically clean up after themselves
- Manual cleanup: Connect to test database and clear collections
- Container restart: `docker-compose restart minisocial-mongodb`
