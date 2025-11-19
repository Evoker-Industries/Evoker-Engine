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

    public override void Load()
    {
        // Load texture from file
    }

    public override void Unload()
    {
        Data = null;
    }
}

/// <summary>
/// Shader resource
/// </summary>
public class Shader : Resource
{
    public string VertexShaderCode { get; set; } = string.Empty;
    public string FragmentShaderCode { get; set; } = string.Empty;

    public override void Load()
    {
        // Load and compile shader
    }

    public override void Unload()
    {
        // Cleanup shader resources
    }
}

/// <summary>
/// Mesh resource
/// </summary>
public class Mesh : Resource
{
    public float[]? Vertices { get; set; }
    public uint[]? Indices { get; set; }

    public override void Load()
    {
        // Load mesh from file
    }

    public override void Unload()
    {
        Vertices = null;
        Indices = null;
    }
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
