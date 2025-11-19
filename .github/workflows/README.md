# GitHub Workflows

This directory contains GitHub Actions workflows for continuous integration, testing, and code quality checks.

## Workflows

### 1. CI/CD Pipeline (`ci.yml`)

**Triggers:** Push and Pull Requests to `main` and `develop` branches

**Jobs:**
- **Build and Test**: Multi-platform build (Ubuntu, Windows, macOS) with .NET 9.0
  - Restores dependencies
  - Builds in Release configuration
  - Runs all 66 unit tests
  - Uploads test results as artifacts

- **Code Quality**: Checks code quality and test coverage
  - Runs tests with code coverage
  - Generates coverage reports
  - Sets quality thresholds (60% minimum, 80% target)

- **Security Scan**: CodeQL security analysis
  - Initializes CodeQL for C#
  - Builds and analyzes code
  - Reports security vulnerabilities

- **Build Documentation**: Validates documentation
  - Checks README.md existence
  - Validates documentation completeness

- **Merge Ready**: Final validation check
  - Ensures all previous jobs succeeded
  - Provides clear pass/fail status

### 2. Pull Request Validation (`pr-validation.yml`)

**Triggers:** Pull request opened, synchronized, or reopened

**Jobs:**
- **PR Checks**: Validates pull request quality
  - Checks PR title length and quality
  - Detects merge conflicts
  - Runs full test suite
  - Verifies no build warnings

- **Test Coverage**: Generates detailed coverage report
  - Runs tests with coverage collection
  - Displays coverage summary
  - Tracks all 66 tests

- **Performance Check**: Validates performance
  - Checks binary sizes
  - Measures test execution time
  - Ensures no performance regressions

- **Final Validation**: Consolidates results
  - Verifies all checks passed
  - Posts status comment on PR
  - Provides clear merge readiness status

### 3. Dependency and Security Check (`dependency-security.yml`)

**Triggers:** Daily at 2 AM UTC, manual dispatch, or when dependencies change

**Jobs:**
- **Dependency Check**: Monitors NuGet packages
  - Checks for outdated packages
  - Identifies vulnerable packages
  - Lists all current dependencies

- **Security Audit**: Security scanning
  - Runs security code scan
  - Checks for hardcoded secrets
  - Identifies security-related TODOs

- **Build Health**: Overall solution health
  - Counts projects and files
  - Builds all configurations
  - Verifies test execution

- **Report Status**: Summarizes results
  - Generates status report
  - Provides actionable feedback

## Setting Up Branch Protection

To enforce these workflows before merging, configure branch protection rules:

### For `main` branch:

1. Go to Settings > Branches > Add rule
2. Branch name pattern: `main`
3. Enable the following:
   - ✅ Require a pull request before merging
   - ✅ Require approvals (1 minimum)
   - ✅ Dismiss stale pull request approvals when new commits are pushed
   - ✅ Require status checks to pass before merging
   - ✅ Require branches to be up to date before merging

4. Required status checks:
   - `Build and Test (ubuntu-latest)`
   - `Build and Test (windows-latest)`
   - `Build and Test (macos-latest)`
   - `Code Quality Checks`
   - `Security Scan`
   - `Build Documentation`
   - `PR Validation`
   - `Test Coverage Report`
   - `Final Validation`

5. Additional settings:
   - ✅ Require conversation resolution before merging
   - ✅ Do not allow bypassing the above settings

### For `develop` branch:

Same as `main` but can optionally reduce required reviewers to 0 for faster iteration.

## Workflow Status Badges

Add these badges to your README.md:

```markdown
[![CI/CD Pipeline](https://github.com/evokerking1/Evoker-Engine/actions/workflows/ci.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/ci.yml)
[![PR Validation](https://github.com/evokerking1/Evoker-Engine/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/pr-validation.yml)
[![Security](https://github.com/evokerking1/Evoker-Engine/actions/workflows/dependency-security.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/dependency-security.yml)
```

## Running Workflows Locally

While you can't run GitHub Actions locally, you can simulate the checks:

```bash
# Build and test
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --verbosity normal

# Check for outdated packages
dotnet list package --outdated

# Check for vulnerabilities
dotnet list package --vulnerable
```

## Troubleshooting

### Tests Failing
- Check test output in the workflow logs
- Run tests locally: `dotnet test --verbosity detailed`
- Ensure all dependencies are up to date

### Build Errors
- Verify .NET 9.0 SDK is installed
- Check for merge conflicts
- Ensure all NuGet packages are restored

### Security Scan Issues
- Review CodeQL alerts in the Security tab
- Address any vulnerable dependencies
- Remove hardcoded secrets or sensitive data

## Maintenance

Workflows are automatically maintained but should be reviewed:
- **Monthly**: Update action versions (e.g., `@v4` → `@v5`)
- **Quarterly**: Review and update .NET versions
- **As needed**: Add new checks or modify thresholds

## Support

For issues with workflows:
1. Check the Actions tab for detailed logs
2. Review the workflow YAML for configuration
3. Consult GitHub Actions documentation
4. Open an issue in the repository
