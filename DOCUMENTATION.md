# Documentation Guide

This guide explains how the Evoker Engine documentation is structured and deployed.

## Documentation Structure

The project uses two documentation systems:

### 1. MkDocs (User Documentation)
Located in `docs/` directory, built with Material for MkDocs theme.

**Content:**
- Getting Started guides
- Game development tutorials (2D and 3D)
- Core systems explanations
- Modding guides
- Contributing guidelines

**Configuration:** `mkdocs.yml`

### 2. DocFX (API Documentation)
Auto-generated from C# XML comments in the source code.

**Content:**
- Complete API reference for all public classes and methods
- Namespace documentation
- Code examples from XML comments

**Configuration:** `docfx.json`

## Building Documentation Locally

### MkDocs

```bash
# Install dependencies
pip install -r requirements.txt

# Serve locally (with auto-reload)
mkdocs serve

# Build static site
mkdocs build
```

The documentation will be available at http://localhost:8000

### DocFX

```bash
# Install DocFX
dotnet tool install -g docfx

# Build API documentation
docfx docfx.json

# Serve locally
docfx docfx.json --serve
```

The API documentation will be available at http://localhost:8080

## Automatic Deployment

Documentation is automatically deployed to GitHub Pages on every push to `main` branch.

**Workflow:** `.github/workflows/deploy-docs.yml`

**Steps:**
1. Builds MkDocs documentation
2. Generates DocFX API documentation
3. Combines both into single site
4. Deploys to GitHub Pages

**URL:** https://evokerking1.github.io/Evoker-Engine/

## Adding New Documentation

### Adding a New Page

1. Create a markdown file in the appropriate `docs/` subdirectory
2. Add it to `nav` section in `mkdocs.yml`
3. Commit and push (will auto-deploy)

### Adding API Documentation

1. Add XML documentation comments to your C# code:

```csharp
/// <summary>
/// Description of the class
/// </summary>
public class MyClass
{
    /// <summary>
    /// Description of the method
    /// </summary>
    /// <param name="value">Parameter description</param>
    /// <returns>Return value description</returns>
    public int MyMethod(int value)
    {
        return value * 2;
    }
}
```

2. DocFX will automatically generate documentation from these comments

## Documentation Dependencies

### Python Dependencies (MkDocs)
See `requirements.txt`:
- mkdocs-material - Material theme for MkDocs
- mkdocs-autorefs - Auto-reference plugin

### .NET Tool (DocFX)
- docfx - API documentation generator

## Troubleshooting

### MkDocs build fails
```bash
# Check Python version (3.8+)
python --version

# Reinstall dependencies
pip install --force-reinstall -r requirements.txt
```

### DocFX build fails
```bash
# Reinstall DocFX
dotnet tool uninstall -g docfx
dotnet tool install -g docfx

# Clean and rebuild
rm -rf _site api-docs/api
dotnet clean
dotnet build
docfx docfx.json
```

### GitHub Pages not updating
1. Check the Actions tab for deployment status
2. Verify GitHub Pages is enabled in repository settings
3. Ensure the workflow has write permissions

## Directory Structure

```
Evoker-Engine/
├── docs/                  # MkDocs source files
│   ├── index.md          # Homepage
│   ├── getting-started/  # Getting started guides
│   ├── game-dev/         # Game development tutorials
│   ├── core/             # Core systems documentation
│   ├── graphics/         # Graphics documentation
│   ├── systems/          # Game systems documentation
│   ├── modding/          # Modding guides
│   ├── api/              # API overview
│   └── contributing.md   # Contributing guide
├── api-docs/             # DocFX source files
│   └── index.md         # API documentation homepage
├── mkdocs.yml           # MkDocs configuration
├── docfx.json           # DocFX configuration
├── requirements.txt     # Python dependencies
└── .github/
    └── workflows/
        └── deploy-docs.yml  # Deployment workflow
```

## Writing Style Guide

### Documentation Best Practices

1. **Use clear, simple language**
2. **Include code examples** for every feature
3. **Add diagrams** where helpful (use Mermaid)
4. **Link related topics** using relative links
5. **Keep pages focused** on one topic

### Code Examples

Always include runnable code examples:

```csharp
// Good: Complete, runnable example
var app = new Application("My Game", 1280, 720);
app.PushLayer(new GameLayer());
app.Run();

// Bad: Incomplete snippet
app.PushLayer(new GameLayer());
```

### Admonitions

Use admonitions for important information:

```markdown
!!! tip "Performance Tip"
    Use object pooling for frequently created entities.

!!! warning "Breaking Change"
    This API will change in version 2.0.

!!! note
    This feature requires .NET 9.0 or later.
```

## Contributing to Documentation

1. Fork the repository
2. Create a branch for your documentation changes
3. Write or update documentation
4. Test locally with `mkdocs serve`
5. Submit a pull request

All documentation contributions are welcome!
