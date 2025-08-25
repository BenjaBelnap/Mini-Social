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
dotnet test
```

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

