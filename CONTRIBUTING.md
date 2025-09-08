# Contributing to Mini-Social

## Development Workflow

We follow a feature branch workflow with comprehensive CI/CD checks.

### Branch Strategy

- `main` - Production-ready code, protected branch
- `develop` - Integration branch for features (optional)
- `feature/*` - Feature development branches
- `fix/*` - Bug fix branches
- `chore/*` - Maintenance and tooling updates

### Creating a Feature

1. **Create a branch from main:**
   ```bash
   git checkout main
   git pull origin main
   git checkout -b feature/your-feature-name
   ```

2. **Follow TDD practices:**
   - Write failing tests first (Red)
   - Implement minimal code to pass (Green)  
   - Refactor and improve (Refactor)

3. **Commit frequently with semantic messages:**
   ```bash
   git commit -m "feat(core): add user authentication service"
   git commit -m "test: add user login validation tests"
   git commit -m "fix(api): handle invalid email format in registration"
   ```

### Commit Message Format

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]
```

**Types:**
- `feat` - New features
- `fix` - Bug fixes
- `docs` - Documentation updates
- `test` - Adding or updating tests
- `refactor` - Code refactoring
- `chore` - Maintenance tasks
- `ci` - CI/CD updates

**Scopes:**
- `core` - Domain entities and business logic
- `api` - Web API controllers and middleware
- `infrastructure` - Data access and external services
- `tests` - Test-related changes

### Pull Request Process

1. **Ensure all tests pass:**
   ```bash
   dotnet test
   ```

2. **Run security scan:**
   ```bash
   dotnet list package --vulnerable --include-transitive
   ```

3. **Push your branch:**
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Create PR with descriptive title and description**

5. **GitHub Actions will automatically:**
   - Run all tests
   - Enforce minimum 80% test coverage
   - Check for vulnerabilities
   - Validate commit message format
   - Generate code coverage reports

### Code Quality Standards

- **Test Coverage:** Minimum 80% line coverage enforced (aim for >90%)
- **Code Style:** Follow C# conventions and .NET guidelines
- **Documentation:** Update README and docs as needed
- **Security:** No vulnerable dependencies allowed

Coverage reports are automatically generated for every PR and available as downloadable artifacts. See [Coverage Documentation](docs/COVERAGE.md) for details.

### Local Development

```bash
# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run tests with coverage
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run specific test class
dotnet test --filter "ClassName=UserTests"

# Generate coverage report locally (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:./coverage-report -reporttypes:"Html"
```

## Questions?

Feel free to open an issue for questions about the development process.
