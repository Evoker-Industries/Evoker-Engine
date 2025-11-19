using System;
using System.Collections.Generic;
using System.IO;
using StbImageSharp;

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
        if (!string.IsNullOrEmpty(FilePath))
        {
            LoadFromFile(FilePath);
        }
        else
        {
            Core.Logger.Warning($"Texture has no file path set: {Name}");
        }
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
        
        if (!File.Exists(path))
        {
            Core.Logger.Error($"Texture file not found: {path}");
            return;
        }

        try
        {
            using var stream = File.OpenRead(path);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            
            Width = image.Width;
            Height = image.Height;
            Data = image.Data;
            Format = TextureFormat.RGBA;
            
            Core.Logger.Info($"Loaded texture: {Name} ({Width}x{Height}, {Format})");
        }
        catch (Exception ex)
        {
            Core.Logger.Error($"Failed to load texture {path}: {ex.Message}");
        }
    }

    /// <summary>
    /// Load texture from memory
    /// </summary>
    public void LoadFromMemory(byte[] imageData)
    {
        try
        {
            var image = ImageResult.FromMemory(imageData, ColorComponents.RedGreenBlueAlpha);
            
            Width = image.Width;
            Height = image.Height;
            Data = image.Data;
            Format = TextureFormat.RGBA;
            
            Core.Logger.Info($"Loaded texture from memory: {Name} ({Width}x{Height})");
        }
        catch (Exception ex)
        {
            Core.Logger.Error($"Failed to load texture from memory: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a solid color texture
    /// </summary>
    public static Texture CreateSolidColor(int width, int height, byte r, byte g, byte b, byte a = 255)
    {
        var texture = new Texture
        {
            Name = $"SolidColor_{r}_{g}_{b}_{a}",
            Width = width,
            Height = height,
            Format = TextureFormat.RGBA,
            Data = new byte[width * height * 4]
        };

        for (int i = 0; i < width * height; i++)
        {
            texture.Data[i * 4 + 0] = r;
            texture.Data[i * 4 + 1] = g;
            texture.Data[i * 4 + 2] = b;
            texture.Data[i * 4 + 3] = a;
        }

        return texture;
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
        if (!string.IsNullOrEmpty(VertexShaderPath) && !string.IsNullOrEmpty(FragmentShaderPath))
        {
            LoadFromFiles(VertexShaderPath, FragmentShaderPath);
        }
        else
        {
            Core.Logger.Warning($"Shader paths not set for: {Name}");
        }
    }

    public override void Unload()
    {
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

        try
        {
            if (File.Exists(vertexPath))
            {
                VertexShaderCode = File.ReadAllText(vertexPath);
                Core.Logger.Info($"Loaded vertex shader: {vertexPath}");
            }
            else
            {
                Core.Logger.Error($"Vertex shader file not found: {vertexPath}");
            }

            if (File.Exists(fragmentPath))
            {
                FragmentShaderCode = File.ReadAllText(fragmentPath);
                Core.Logger.Info($"Loaded fragment shader: {fragmentPath}");
            }
            else
            {
                Core.Logger.Error($"Fragment shader file not found: {fragmentPath}");
            }

            // Mark as ready for compilation
            if (!string.IsNullOrEmpty(VertexShaderCode) && !string.IsNullOrEmpty(FragmentShaderCode))
            {
                Core.Logger.Info($"Shader {Name} loaded and ready for compilation");
            }
        }
        catch (Exception ex)
        {
            Core.Logger.Error($"Failed to load shader files: {ex.Message}");
        }
    }

    /// <summary>
    /// Compile shader source code
    /// </summary>
    public bool Compile()
    {
        // TODO: Implement shader compilation to SPIR-V
        // This would require integration with a SPIR-V compiler like glslang or shaderc
        // For now, we'll just validate that we have shader code loaded
        if (string.IsNullOrEmpty(VertexShaderCode) || string.IsNullOrEmpty(FragmentShaderCode))
        {
            Core.Logger.Error($"Cannot compile shader {Name}: Missing shader code");
            return false;
        }

        Core.Logger.Warning($"SPIR-V compilation not yet implemented for: {Name}. Shader code is loaded and ready.");
        IsCompiled = true; // Mark as compiled for testing purposes
        return true;
    }

    /// <summary>
    /// Load shader from source strings
    /// </summary>
    public void LoadFromSource(string vertexSource, string fragmentSource)
    {
        VertexShaderCode = vertexSource;
        FragmentShaderCode = fragmentSource;
        Core.Logger.Info($"Shader {Name} loaded from source strings");
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
        
        // Define cube vertices (position only, 8 vertices)
        mesh.Vertices = new float[]
        {
            // Front face
            -s, -s,  s,  // 0
             s, -s,  s,  // 1
             s,  s,  s,  // 2
            -s,  s,  s,  // 3
            // Back face
            -s, -s, -s,  // 4
             s, -s, -s,  // 5
             s,  s, -s,  // 6
            -s,  s, -s   // 7
        };

        // Define cube indices (36 indices for 12 triangles, 6 faces)
        mesh.Indices = new uint[]
        {
            // Front
            0, 1, 2, 2, 3, 0,
            // Right
            1, 5, 6, 6, 2, 1,
            // Back
            5, 4, 7, 7, 6, 5,
            // Left
            4, 0, 3, 3, 7, 4,
            // Top
            3, 2, 6, 6, 7, 3,
            // Bottom
            4, 5, 1, 1, 0, 4
        };

        // Generate normals (simple face normals)
        mesh.Normals = new float[]
        {
            // Front
            0, 0, 1,  0, 0, 1,  0, 0, 1,  0, 0, 1,
            // Back
            0, 0, -1,  0, 0, -1,  0, 0, -1,  0, 0, -1
        };

        // Generate texture coordinates
        mesh.TexCoords = new float[]
        {
            0, 0,  1, 0,  1, 1,  0, 1,  // Front
            0, 0,  1, 0,  1, 1,  0, 1   // Back
        };

        Core.Logger.Info($"Created cube mesh with {mesh.Vertices.Length / 3} vertices");
        return mesh;
    }

    /// <summary>
    /// Create a simple plane mesh
    /// </summary>
    public static Mesh CreatePlane(float width = 1.0f, float height = 1.0f)
    {
        var mesh = new Mesh { Name = "Plane" };
        float w = width / 2f;
        float h = height / 2f;
        
        // Define plane vertices (4 vertices)
        mesh.Vertices = new float[]
        {
            -w, 0, -h,  // 0 - Bottom left
             w, 0, -h,  // 1 - Bottom right
             w, 0,  h,  // 2 - Top right
            -w, 0,  h   // 3 - Top left
        };

        // Define plane indices (2 triangles)
        mesh.Indices = new uint[]
        {
            0, 1, 2,  // First triangle
            2, 3, 0   // Second triangle
        };

        // Normals pointing up
        mesh.Normals = new float[]
        {
            0, 1, 0,
            0, 1, 0,
            0, 1, 0,
            0, 1, 0
        };

        // Texture coordinates
        mesh.TexCoords = new float[]
        {
            0, 0,  // Bottom left
            1, 0,  // Bottom right
            1, 1,  // Top right
            0, 1   // Top left
        };

        Core.Logger.Info($"Created plane mesh ({width}x{height})");
        return mesh;
    }

    /// <summary>
    /// Create a sphere mesh
    /// </summary>
    public static Mesh CreateSphere(float radius = 0.5f, int segments = 16, int rings = 16)
    {
        var mesh = new Mesh { Name = "Sphere" };
        
        var vertices = new List<float>();
        var normals = new List<float>();
        var texCoords = new List<float>();
        var indices = new List<uint>();

        // Generate vertices
        for (int ring = 0; ring <= rings; ring++)
        {
            float v = (float)ring / rings;
            float phi = v * MathF.PI;

            for (int seg = 0; seg <= segments; seg++)
            {
                float u = (float)seg / segments;
                float theta = u * MathF.PI * 2f;

                float x = MathF.Cos(theta) * MathF.Sin(phi);
                float y = MathF.Cos(phi);
                float z = MathF.Sin(theta) * MathF.Sin(phi);

                vertices.Add(x * radius);
                vertices.Add(y * radius);
                vertices.Add(z * radius);

                normals.Add(x);
                normals.Add(y);
                normals.Add(z);

                texCoords.Add(u);
                texCoords.Add(v);
            }
        }

        // Generate indices
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                uint current = (uint)(ring * (segments + 1) + seg);
                uint next = current + (uint)(segments + 1);

                indices.Add(current);
                indices.Add(next);
                indices.Add(current + 1);

                indices.Add(current + 1);
                indices.Add(next);
                indices.Add(next + 1);
            }
        }

        mesh.Vertices = vertices.ToArray();
        mesh.Normals = normals.ToArray();
        mesh.TexCoords = texCoords.ToArray();
        mesh.Indices = indices.ToArray();

        Core.Logger.Info($"Created sphere mesh with {mesh.Vertices.Length / 3} vertices");
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
