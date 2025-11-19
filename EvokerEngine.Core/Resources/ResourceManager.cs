using System;
using System.Collections.Generic;

namespace EvokerEngine.Resources;

/// <summary>
/// Base class for all resources
/// </summary>
public abstract class Resource
{
    public int Id { get; internal set; }
    public string Name { get; set; } = string.Empty;
    
    public abstract void Load();
    public abstract void Unload();
}

/// <summary>
/// Texture resource
/// </summary>
public class Texture : Resource
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[]? Data { get; set; }
    public TextureFormat Format { get; set; } = TextureFormat.RGBA;
    public string? FilePath { get; set; }

    public override void Load()
    {
        // TODO: Implement texture loading from file
        // Supported formats: PNG, JPG, BMP, TGA
        // This will be implemented with image loading library (e.g., StbImageSharp)
        Core.Logger.Warning($"Texture loading not yet implemented for: {Name}");
    }

    public override void Unload()
    {
        Data = null;
    }

    /// <summary>
    /// Load texture from file path
    /// </summary>
    public void LoadFromFile(string path)
    {
        FilePath = path;
        // TODO: Implement file loading
        // Will support: .png, .jpg, .jpeg, .bmp, .tga
        Core.Logger.Warning($"LoadFromFile not yet implemented for: {path}");
    }
}

/// <summary>
/// Texture format
/// </summary>
public enum TextureFormat
{
    RGB,
    RGBA,
    Grayscale,
    GrayscaleAlpha
}

/// <summary>
/// Shader resource
/// </summary>
public class Shader : Resource
{
    public string VertexShaderCode { get; set; } = string.Empty;
    public string FragmentShaderCode { get; set; } = string.Empty;
    public string? VertexShaderPath { get; set; }
    public string? FragmentShaderPath { get; set; }
    public bool IsCompiled { get; private set; }

    public override void Load()
    {
        // TODO: Implement shader compilation
        // Will support GLSL/SPIR-V shader compilation
        Core.Logger.Warning($"Shader compilation not yet implemented for: {Name}");
    }

    public override void Unload()
    {
        // TODO: Cleanup shader resources
        VertexShaderCode = string.Empty;
        FragmentShaderCode = string.Empty;
        IsCompiled = false;
    }

    /// <summary>
    /// Load and compile shader from files
    /// </summary>
    public void LoadFromFiles(string vertexPath, string fragmentPath)
    {
        VertexShaderPath = vertexPath;
        FragmentShaderPath = fragmentPath;
        // TODO: Read files and compile
        Core.Logger.Warning($"Shader LoadFromFiles not yet implemented: {vertexPath}, {fragmentPath}");
    }

    /// <summary>
    /// Compile shader source code
    /// </summary>
    public bool Compile()
    {
        // TODO: Implement shader compilation to SPIR-V
        Core.Logger.Warning($"Shader compilation not yet implemented for: {Name}");
        return false;
    }
}

/// <summary>
/// Mesh resource
/// </summary>
public class Mesh : Resource
{
    public float[]? Vertices { get; set; }
    public uint[]? Indices { get; set; }
    public float[]? Normals { get; set; }
    public float[]? TexCoords { get; set; }
    public string? FilePath { get; set; }
    public MeshFormat Format { get; set; } = MeshFormat.Custom;

    public override void Load()
    {
        // TODO: Implement mesh loading from file
        // Supported formats: GLTF, GLB, OBJ
        Core.Logger.Warning($"Mesh loading not yet implemented for: {Name}");
    }

    public override void Unload()
    {
        Vertices = null;
        Indices = null;
        Normals = null;
        TexCoords = null;
    }

    /// <summary>
    /// Load mesh from file
    /// </summary>
    public void LoadFromFile(string path)
    {
        FilePath = path;
        
        // Detect format from extension
        if (path.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase))
            Format = MeshFormat.GLTF;
        else if (path.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            Format = MeshFormat.GLB;
        else if (path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
            Format = MeshFormat.OBJ;
        
        // TODO: Implement file loading based on format
        Core.Logger.Warning($"Mesh LoadFromFile not yet implemented for: {path}");
    }

    /// <summary>
    /// Create a simple cube mesh
    /// </summary>
    public static Mesh CreateCube(float size = 1.0f)
    {
        var mesh = new Mesh { Name = "Cube" };
        float s = size / 2f;
        
        // TODO: Implement cube mesh generation
        Core.Logger.Warning("Mesh.CreateCube not yet fully implemented");
        
        return mesh;
    }

    /// <summary>
    /// Create a simple plane mesh
    /// </summary>
    public static Mesh CreatePlane(float width = 1.0f, float height = 1.0f)
    {
        var mesh = new Mesh { Name = "Plane" };
        
        // TODO: Implement plane mesh generation
        Core.Logger.Warning("Mesh.CreatePlane not yet fully implemented");
        
        return mesh;
    }
}

/// <summary>
/// Mesh file format
/// </summary>
public enum MeshFormat
{
    Custom,
    GLTF,
    GLB,
    OBJ
}

/// <summary>
/// Resource manager for loading and managing resources
/// </summary>
public class ResourceManager
{
    private readonly Dictionary<int, Resource> _resources = new();
    private int _nextResourceId = 1;

    /// <summary>
    /// Load a resource
    /// </summary>
    public T Load<T>(string name) where T : Resource, new()
    {
        var resource = new T
        {
            Id = _nextResourceId++,
            Name = name
        };

        resource.Load();
        _resources[resource.Id] = resource;
        
        return resource;
    }

    /// <summary>
    /// Get a resource by ID
    /// </summary>
    public T? Get<T>(int id) where T : Resource
    {
        if (_resources.TryGetValue(id, out var resource))
        {
            return resource as T;
        }
        return null;
    }

    /// <summary>
    /// Unload a resource
    /// </summary>
    public void Unload(int id)
    {
        if (_resources.TryGetValue(id, out var resource))
        {
            resource.Unload();
            _resources.Remove(id);
        }
    }

    /// <summary>
    /// Unload all resources
    /// </summary>
    public void UnloadAll()
    {
        foreach (var resource in _resources.Values)
        {
            resource.Unload();
        }
        _resources.Clear();
    }
}
