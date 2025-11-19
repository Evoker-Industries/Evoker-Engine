using System;
using System.Collections.Generic;
using System.Numerics;
using EvokerEngine.Core;

namespace EvokerEngine.World;

/// <summary>
/// Represents a dimension/world in the game
/// </summary>
public class Dimension
{
    /// <summary>
    /// Unique identifier for this dimension
    /// </summary>
    public ResourceKey Id { get; set; }

    /// <summary>
    /// Display name of the dimension
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dimension type (overworld, nether, end, custom, etc.)
    /// </summary>
    public DimensionType Type { get; set; } = DimensionType.Custom;

    /// <summary>
    /// Ambient light level (0-15)
    /// </summary>
    public int AmbientLight { get; set; } = 15;

    /// <summary>
    /// Whether the dimension has a sky
    /// </summary>
    public bool HasSky { get; set; } = true;

    /// <summary>
    /// Whether the dimension has a ceiling
    /// </summary>
    public bool HasCeiling { get; set; } = false;

    /// <summary>
    /// Fog color
    /// </summary>
    public Vector3 FogColor { get; set; } = new Vector3(0.8f, 0.8f, 1.0f);

    /// <summary>
    /// Sky color
    /// </summary>
    public Vector3 SkyColor { get; set; } = new Vector3(0.5f, 0.7f, 1.0f);

    /// <summary>
    /// Coordinate scale relative to overworld (e.g., Nether is 8:1)
    /// </summary>
    public float CoordinateScale { get; set; } = 1.0f;

    /// <summary>
    /// Minimum build height
    /// </summary>
    public int MinHeight { get; set; } = -64;

    /// <summary>
    /// Maximum build height
    /// </summary>
    public int MaxHeight { get; set; } = 320;

    /// <summary>
    /// Total height of the dimension
    /// </summary>
    public int Height => MaxHeight - MinHeight;

    /// <summary>
    /// Whether beds work in this dimension
    /// </summary>
    public bool BedsWork { get; set; } = true;

    /// <summary>
    /// Whether respawn anchors work in this dimension
    /// </summary>
    public bool RespawnAnchorsWork { get; set; } = false;

    /// <summary>
    /// Whether water evaporates in this dimension
    /// </summary>
    public bool WaterEvaporates { get; set; } = false;

    /// <summary>
    /// Whether lava spreads faster in this dimension
    /// </summary>
    public bool LavaSpreadsWildly { get; set; } = false;

    /// <summary>
    /// Whether the dimension has raids
    /// </summary>
    public bool HasRaids { get; set; } = true;

    /// <summary>
    /// Gravity multiplier (1.0 = normal)
    /// </summary>
    public float Gravity { get; set; } = 1.0f;

    /// <summary>
    /// Custom properties for dimension-specific data
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Called when the dimension is loaded
    /// </summary>
    public virtual void OnLoad()
    {
    }

    /// <summary>
    /// Called when the dimension is unloaded
    /// </summary>
    public virtual void OnUnload()
    {
    }

    /// <summary>
    /// Called every tick for dimension updates
    /// </summary>
    public virtual void OnUpdate(float deltaTime)
    {
    }

    /// <summary>
    /// Convert coordinates from this dimension to another
    /// </summary>
    public Vector3 ConvertCoordinates(Vector3 position, Dimension targetDimension)
    {
        var scale = CoordinateScale / targetDimension.CoordinateScale;
        return position * scale;
    }

    /// <summary>
    /// Check if a position is within the dimension's height limits
    /// </summary>
    public bool IsWithinHeightLimits(float y)
    {
        return y >= MinHeight && y <= MaxHeight;
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}

/// <summary>
/// Dimension types
/// </summary>
public enum DimensionType
{
    Overworld,
    Nether,
    End,
    Custom
}

/// <summary>
/// Simple dimension implementation
/// </summary>
public class SimpleDimension : Dimension
{
    public SimpleDimension(ResourceKey id, string name)
    {
        Id = id;
        Name = name;
    }

    public SimpleDimension(string namespacePart, string key, string name)
        : this(new ResourceKey(namespacePart, key), name)
    {
    }
}
