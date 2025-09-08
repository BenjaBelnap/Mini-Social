# Test Coverage Enforcement

This document explains the test coverage enforcement system implemented in the Mini-Social project's CI/CD pipeline.

## Overview

The branch protection workflow automatically enforces minimum test coverage thresholds to ensure code quality standards are met before merging pull requests into the main branch.

## How It Works

### 1. Coverage Collection
During the test execution phase, coverage data is collected using the .NET XPlat Code Coverage collector:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### 2. Report Generation
The workflow uses [ReportGenerator](https://github.com/danielpalme/ReportGenerator) to:
- Parse XML coverage data from multiple test projects
- Generate human-readable HTML reports
- Create JSON summaries for automated parsing
- Generate coverage badges

### 3. Threshold Enforcement
The system:
- **Current Threshold**: 80% line coverage minimum
- **Enforcement**: Fails the PR if coverage is below threshold
- **Feedback**: Provides clear success/failure messages with actual percentages

### 4. Artifact Storage
Coverage reports are automatically uploaded as GitHub Actions artifacts with 30-day retention:
- **coverage-report**: HTML reports and summaries
- **coverage-data**: Raw XML coverage files

## Workflow Steps

### Install ReportGenerator
```yaml
- name: Install ReportGenerator
  if: always()
  run: dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Generate Coverage Report
```yaml
- name: Generate Coverage Report
  if: always()
  run: |
    # Find coverage files
    COVERAGE_FILES=$(find ./coverage -name "coverage.cobertura.xml" -o -name "*.xml" | grep -E "(coverage|cobertura)")
    
    # Generate reports
    reportgenerator \
      -reports:"$COVERAGE_FILES" \
      -targetdir:./coverage-report \
      -reporttypes:"Html;JsonSummary;Badges"
```

### Parse and Enforce Thresholds
```yaml
- name: Parse Coverage and Enforce Thresholds
  if: always()
  run: |
    MIN_COVERAGE=80
    COVERAGE=$(cat ./coverage-report/Summary.json | grep -o '"linecoverage":"[0-9.]*"' | cut -d'"' -f4)
    
    if [ "$COVERAGE_INT" -lt "$MIN_COVERAGE" ]; then
      echo "❌ COVERAGE BELOW THRESHOLD!"
      exit 1
    fi
```

## Coverage Output Examples

### ✅ Successful Coverage Check
```
📊 Current Coverage: 85.7%
🎯 Required Coverage: 80%

✅ COVERAGE THRESHOLD MET!
   Current: 85.7%
   Required: 80%
   Excess: 5%
```

### ❌ Failed Coverage Check
```
📊 Current Coverage: 72.3%
🎯 Required Coverage: 80%

❌ COVERAGE BELOW THRESHOLD!
   Current: 72.3%
   Required: 80%
   Shortfall: 8%

Please add more tests to increase coverage before merging.
```

## Error Handling

The system gracefully handles various error scenarios:

### Missing Coverage Files
```
❌ No coverage files found in ./coverage directory
Contents of coverage directory:
[directory listing or "Coverage directory does not exist"]
```

### Invalid Coverage Data
```
❌ Could not parse coverage percentage
```

### Missing Summary File
```
❌ Coverage summary file not found
```

## Configuration

### Adjusting Coverage Threshold
To modify the minimum coverage threshold, update the `MIN_COVERAGE` variable in the workflow:

```yaml
# Set minimum coverage threshold (configurable)
MIN_COVERAGE=85  # Change from 80 to 85
```

### Future Enhancements
Consider making the threshold configurable via:
- Environment variables
- Workflow inputs
- Repository secrets
- Configuration files

## Viewing Coverage Reports

### During PR Review
1. Navigate to the failed/successful workflow run
2. Download the `coverage-report` artifact
3. Extract and open `index.html` in a browser

### Local Development
Generate coverage reports locally:
```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Install ReportGenerator (if not already installed)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate report
reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:./coverage-report \
  -reporttypes:"Html"

# Open report
open ./coverage-report/index.html  # macOS
start ./coverage-report/index.html # Windows
```

## Best Practices

### Writing Testable Code
- Use dependency injection for easier mocking
- Keep methods focused and small
- Separate business logic from infrastructure concerns

### Improving Coverage
- Focus on critical business logic first
- Use test-driven development (TDD)
- Write integration tests for complex workflows
- Don't sacrifice test quality for coverage metrics

### Coverage vs Quality
Remember that 100% coverage doesn't guarantee bug-free code:
- Focus on meaningful tests that verify behavior
- Use coverage as a guide, not an absolute rule
- Consider edge cases and error conditions
- Test both happy path and failure scenarios

## Troubleshooting

### Workflow Fails to Find Coverage Files
1. Verify tests are running successfully
2. Check that `--collect:"XPlat Code Coverage"` is specified
3. Ensure `--results-directory ./coverage` is set
4. Look for XML files in the coverage directory

### Coverage Percentage Seems Wrong
1. Verify all test projects are included
2. Check for test project exclusions
3. Review ReportGenerator verbosity output
4. Examine the generated Summary.json file

### ReportGenerator Installation Fails
1. Check .NET SDK version compatibility
2. Verify internet connectivity in CI environment
3. Consider using a specific version: `dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.2.0`

## References

- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
- [.NET Code Coverage](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)
- [XPlat Code Coverage](https://github.com/Microsoft/vstest-docs/blob/master/docs/analyze.md#coverage)
