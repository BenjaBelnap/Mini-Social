# Mini-Social

[![CI/CD Pipeline](https://github.com/BenjaBelnap/Mini-Social/actions/workflows/branch-protection.yml/badge.svg)](https://github.com/BenjaBelnap/Mini-Social/actions/workflows/branch-protection.yml)

A lightweight social media platform built with C# and MongoDB, following test-driven development practices.

## Features

- User management with profile customization
- Post creation with hashtag and mention support
- Follow/unfollow functionality
- Nested comment system
- Real-time engagement tracking

## Tech Stack

- **Backend**: ASP.NET Core 9.0
- **Database**: MongoDB
- **Authentication**: JWT Bearer tokens
- **Testing**: xUnit, FluentAssertions, Moq
- **CI/CD**: GitHub Actions

## Development

### Prerequisites

- .NET 9.0 SDK
- MongoDB (local or Atlas)

### Running Tests

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "FullyQualifiedName!~Integration"

# Run only integration tests  
dotnet test --filter "FullyQualifiedName~Integration"
```

For detailed testing information, see [Testing Guide](docs/TESTING.md).

### Database Migrations

The project uses a C# migration system for database schema management:

```bash
# Run all pending migrations
dotnet run --project src/MiniSocial.Api migrate

# Check migration status
dotnet run --project src/MiniSocial.Api migrate --status

# Migrate to specific version
dotnet run --project src/MiniSocial.Api migrate --target-version 002
```

**Key Benefits:**
- ✅ Migrate from any version to current
- ✅ Skip already applied migrations
- ✅ Version tracking in database
- ✅ Rollback support (via Down methods)
- ✅ Type-safe C# migrations

### Building the Project

```bash
dotnet build
```

## Project Structure

```
├── src/
│   ├── MiniSocial.Api/          # Web API controllers
│   ├── MiniSocial.Core/         # Domain entities and business logic
│   └── MiniSocial.Infrastructure/ # Data access and external services
├── tests/
│   └── MiniSocial.Tests/        # Unit and integration tests
└── .github/workflows/           # CI/CD pipelines
```

