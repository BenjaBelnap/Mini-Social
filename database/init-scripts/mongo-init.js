// MongoDB initialization script
// This script runs when the MongoDB container starts for the first time
// It only creates users - schema creation is handled by C# migrations

print('Starting MongoDB initialization...');

// Switch to the minisocial database
db = db.getSiblingDB('minisocial');

print('Creating application user...');

// Create application user with read/write permissions on minisocial database
db.createUser({
  user: 'minisocial_user',
  pwd: 'minisocial_password',
  roles: [
    {
      role: 'readWrite',
      db: 'minisocial'
    },
    {
      role: 'readWriteAnyDatabase',
      db: 'admin'
    }
  ]
});

print('Application user created successfully.');

// Switch to the test database
db = db.getSiblingDB('minisocial_test');

print('Creating test user...');

// Create test user with read/write permissions and index creation permissions on test database only
db.createUser({
  user: 'minisocial_test_user',
  pwd: 'minisocial_test_password',
  roles: [
    {
      role: 'dbOwner',
      db: 'minisocial_test'
    }
  ]
});

print('Test user created successfully.');

print('MongoDB initialization completed!');
print('Note: Database schema will be created by C# migrations when the application starts.');
print('To run migrations manually, use: dotnet run migrate');
