// MongoDB initialization script
// This script runs when the MongoDB container starts for the first time

print('Starting MongoDB initialization...');

// Switch to the minisocial database
db = db.getSiblingDB('minisocial');

print('Creating application user...');

// Create application user with read/write permissions on all databases
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

print('Creating collections with validation schemas...');
db.createCollection('users', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['_id', 'username', 'email', 'createdAt'],
      properties: {
        _id: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        username: {
          bsonType: 'string',
          minLength: 3,
          maxLength: 50,
          description: 'must be a string between 3-50 characters and is required'
        },
        email: {
          bsonType: 'string',
          pattern: '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
          description: 'must be a valid email address and is required'
        },
        displayName: {
          bsonType: 'string',
          maxLength: 100,
          description: 'must be a string with max 100 characters'
        },
        bio: {
          bsonType: 'string',
          maxLength: 500,
          description: 'must be a string with max 500 characters'
        },
        createdAt: {
          bsonType: 'date',
          description: 'must be a date and is required'
        },
        updatedAt: {
          bsonType: 'date',
          description: 'must be a date'
        }
      }
    }
  }
});

db.createCollection('posts', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['id', 'userId', 'content', 'createdAt'],
      properties: {
        id: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        userId: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        content: {
          bsonType: 'string',
          minLength: 1,
          maxLength: 1000,
          description: 'must be a string between 1-1000 characters and is required'
        },
        createdAt: {
          bsonType: 'date',
          description: 'must be a date and is required'
        },
        updatedAt: {
          bsonType: 'date',
          description: 'must be a date'
        }
      }
    }
  }
});

db.createCollection('comments', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['id', 'postId', 'userId', 'content', 'createdAt'],
      properties: {
        id: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        postId: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        userId: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        content: {
          bsonType: 'string',
          minLength: 1,
          maxLength: 500,
          description: 'must be a string between 1-500 characters and is required'
        },
        createdAt: {
          bsonType: 'date',
          description: 'must be a date and is required'
        },
        updatedAt: {
          bsonType: 'date',
          description: 'must be a date'
        }
      }
    }
  }
});

db.createCollection('follows', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['id', 'followerId', 'followeeId', 'createdAt'],
      properties: {
        id: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        followerId: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        followeeId: {
          bsonType: 'string',
          description: 'must be a string and is required'
        },
        createdAt: {
          bsonType: 'date',
          description: 'must be a date and is required'
        }
      }
    }
  }
});

// Create indexes for better performance
db.users.createIndex({ 'username': 1 }, { unique: true });
db.users.createIndex({ 'email': 1 }, { unique: true });
db.posts.createIndex({ 'userId': 1 });
db.posts.createIndex({ 'createdAt': -1 });
db.comments.createIndex({ 'postId': 1 });
db.comments.createIndex({ 'userId': 1 });
db.follows.createIndex({ 'followerId': 1, 'followeeId': 1 }, { unique: true });
db.follows.createIndex({ 'followerId': 1 });
db.follows.createIndex({ 'followeeId': 1 });

print('Database initialization completed successfully!');
