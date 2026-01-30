# NuGet Package Publishing

This document describes how to publish the EvokerEngine.Core NuGet package to the custom repository.

## Overview

The Evoker Engine automatically publishes NuGet packages to the custom repository at:
`https://repo.evokerking.dev/repository/nuget-hosted/`

## Automatic Publishing

### Via Git Tags

The easiest way to publish a new version is to create and push a git tag:

```bash
# Create a tag with the version number (must start with 'v')
git tag v1.0.0

# Push the tag to GitHub
git push origin v1.0.0
```

This will automatically trigger the publish workflow which will:
1. Extract the version from the tag (removing the 'v' prefix)
2. Update the project version in the .csproj file
3. Restore dependencies
4. Build the project in Release configuration
5. Run all tests
6. Pack the NuGet package
7. Publish to the NuGet repository
8. Upload the package as an artifact

### Via Manual Workflow Dispatch

You can also manually trigger the workflow from GitHub Actions:

1. Go to the repository on GitHub
2. Click on "Actions" tab
3. Select "Publish NuGet Package" workflow
4. Click "Run workflow"
5. Enter the version number (e.g., `1.0.0` without the 'v' prefix)
6. Click "Run workflow"

## Setup Requirements

### GitHub Secret

The repository requires a GitHub secret named `NUGET_API_KEY` to publish packages.

To set this up:

1. Generate an API key from your NuGet repository (https://repo.evokerking.dev/)
2. Go to the GitHub repository settings
3. Navigate to "Secrets and variables" → "Actions"
4. Click "New repository secret"
5. Name: `NUGET_API_KEY`
6. Value: Paste your API key
7. Click "Add secret"

## Package Metadata

The package metadata is configured in `EvokerEngine.Core/EvokerEngine.Core.csproj`:

```xml
<PropertyGroup>
  <PackageId>EvokerEngine.Core</PackageId>
  <Version>1.0.0</Version>
  <Authors>Evoker Industries</Authors>
  <Company>Evoker Industries</Company>
  <Description>A complete C# game engine with Vulkan rendering support using Silk.NET</Description>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/Evoker-Industries/Evoker-Engine</PackageProjectUrl>
  <RepositoryUrl>https://github.com/Evoker-Industries/Evoker-Engine</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageTags>game-engine;vulkan;silk-net;gamedev;ecs;rendering</PackageTags>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>
```

The version in the .csproj file serves as the default version but is automatically updated when publishing via tags or manual workflow.

## Version Management

Follow [Semantic Versioning](https://semver.org/) guidelines:

- **Major version** (X.0.0): Breaking changes
- **Minor version** (1.X.0): New features, backward compatible
- **Patch version** (1.0.X): Bug fixes, backward compatible

Example version progression:
- `v1.0.0` - Initial release
- `v1.0.1` - Bug fix
- `v1.1.0` - New feature
- `v2.0.0` - Breaking change

## Workflow File

The publish workflow is defined in `.github/workflows/publish-nuget.yml`.

## Troubleshooting

### Package Already Exists

If you get an error that the package already exists, the workflow will skip it (due to `--skip-duplicate` flag).

### Authentication Failed

If authentication fails:
1. Verify the `NUGET_API_KEY` secret is correctly set
2. Check that the API key has write permissions
3. Ensure the API key hasn't expired

### Build or Test Failures

The workflow will fail if:
- The build fails
- Any tests fail

Fix the issues and create a new tag/version to retry.

## Testing Package Creation

To test package creation locally without publishing:

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build EvokerEngine.Core/EvokerEngine.Core.csproj --configuration Release

# Pack (creates .nupkg file)
dotnet pack EvokerEngine.Core/EvokerEngine.Core.csproj --configuration Release --output ./nupkg

# Inspect the created package
ls -lh ./nupkg/
```

The created `.nupkg` file can be inspected or manually uploaded if needed.
