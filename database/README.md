# Database Directory

This directory contains all MongoDB-related configuration and setup files.

## Structure

```
database/
├── docker-compose.yml          # MongoDB container configuration
├── init-scripts/
│   └── mongo-init.js          # Database initialization script
├── .env.example               # Environment variables template
├── .env                       # Local environment variables (git-ignored)
└── README.md                  # This file
```

## Quick Start

1. **Copy environment variables:**
   ```bash
   cp .env.example .env
   ```

2. **Edit the `.env` file with your secure passwords:**
   ```bash
   # Update these values:
   MONGO_ROOT_PASSWORD=your_secure_root_password_here
   MONGO_APP_PASSWORD=your_secure_app_password_here
   ```

3. **Start MongoDB:**
   ```bash
   docker-compose up -d
   ```

4. **Verify the setup:**
   - MongoDB will be available at `localhost:27017`
   - MongoDB Express (web UI) at `http://localhost:8081`

## Features

- **Automatic Database Setup**: The `mongo-init.js` script creates:
  - Application user with proper permissions for production database
  - Test user with proper permissions for test database 
  - Collections with validation schemas for both databases
  - Performance indexes for both databases

- **Test Isolation**: 
  - Separate test database (`minisocial_test`) with dedicated test user
  - Consolidated environment configuration in single `.env` file
  - Tests clean up data after execution while preserving database structure
  - Integration tests run sequentially to prevent conflicts

- **Security**: 
  - Separate admin and application users
  - Schema validation for data integrity
  - Environment-based configuration

- **Development Tools**:
  - MongoDB Express for database management
  - Structured logging and initialization feedback

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `MONGO_ROOT_USERNAME` | MongoDB admin username | `admin` |
| `MONGO_ROOT_PASSWORD` | MongoDB admin password | `secure_password` |
| `MONGO_DATABASE` | Application database name | `minisocial` |
| `MONGO_TEST_DATABASE` | Test database name | `minisocial_test` |
| `MONGO_APP_USERNAME` | Application user | `minisocial_user` |
| `MONGO_APP_PASSWORD` | Application password | `app_password` |
| `MONGO_TEST_USERNAME` | Test database user | `minisocial_test_user` |
| `MONGO_TEST_PASSWORD` | Test database password | `minisocial_test_password` |
| `MONGO_TEST_CONNECTION_STRING` | Complete test DB connection string | `mongodb://test_user:test_pass@localhost:27017/test_db?authSource=test_db` |

## Cloud Migration

This setup is designed for easy cloud migration:

- **Local**: Uses Docker Compose with environment variables
- **Cloud**: Same environment variables work with:
  - Azure Container Instances
  - AWS ECS/Fargate
  - Google Cloud Run
  - Kubernetes deployments

## Security Best Practices

1. **Never commit `.env` files** (already in .gitignore)
2. **Use strong passwords** for production
3. **Rotate credentials regularly**
4. **Use cloud secret managers** for production:
   - Azure Key Vault
   - AWS Secrets Manager
   - Google Secret Manager
   - HashiCorp Vault
