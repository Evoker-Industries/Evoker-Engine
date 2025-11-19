using System;
using System.Collections.Generic;
using System.Linq;
using EvokerEngine.Core;

namespace EvokerEngine.World;

/// <summary>
/// Registry for managing dimensions
/// </summary>
public class DimensionRegistry
{
    private static DimensionRegistry? _instance;
    private readonly Dictionary<ResourceKey, Dimension> _dimensions = new();
    private Dimension? _defaultDimension;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static DimensionRegistry Instance => _instance ??= new DimensionRegistry();

    /// <summary>
    /// Default/spawn dimension
    /// </summary>
    public Dimension? DefaultDimension
    {
        get => _defaultDimension;
        set
        {
            if (value != null && !_dimensions.ContainsValue(value))
            {
                throw new InvalidOperationException("Cannot set unregistered dimension as default");
            }
            _defaultDimension = value;
        }
    }

    private DimensionRegistry()
    {
        // Register default dimensions
        RegisterDefaultDimensions();
    }

    private void RegisterDefaultDimensions()
    {
        // Overworld
        var overworld = new SimpleDimension("evoker", "overworld", "Overworld")
        {
            Type = DimensionType.Overworld,
            HasSky = true,
            HasCeiling = false,
            MinHeight = -64,
            MaxHeight = 320,
            CoordinateScale = 1.0f,
            BedsWork = true,
            RespawnAnchorsWork = false,
            WaterEvaporates = false,
            HasRaids = true
        };
        Register(overworld);
        DefaultDimension = overworld;

        // Nether
        var nether = new SimpleDimension("evoker", "nether", "The Nether")
        {
            Type = DimensionType.Nether,
            HasSky = false,
            HasCeiling = true,
            MinHeight = 0,
            MaxHeight = 256,
            CoordinateScale = 8.0f,
            AmbientLight = 8,
            FogColor = new System.Numerics.Vector3(0.2f, 0.03f, 0.03f),
            SkyColor = new System.Numerics.Vector3(0.2f, 0.03f, 0.03f),
            BedsWork = false,
            RespawnAnchorsWork = true,
            WaterEvaporates = true,
            LavaSpreadsWildly = true,
            HasRaids = false
        };
        Register(nether);

        // End
        var end = new SimpleDimension("evoker", "end", "The End")
        {
            Type = DimensionType.End,
            HasSky = true,
            HasCeiling = false,
            MinHeight = 0,
            MaxHeight = 256,
            CoordinateScale = 1.0f,
            AmbientLight = 0,
            FogColor = new System.Numerics.Vector3(0.07f, 0.02f, 0.15f),
            SkyColor = new System.Numerics.Vector3(0.07f, 0.02f, 0.15f),
            BedsWork = false,
            RespawnAnchorsWork = false,
            WaterEvaporates = false,
            HasRaids = false
        };
        Register(end);
    }

    /// <summary>
    /// Register a new dimension
    /// </summary>
    public void Register(Dimension dimension)
    {
        if (dimension == null)
            throw new ArgumentNullException(nameof(dimension));

        if (_dimensions.ContainsKey(dimension.Id))
            throw new InvalidOperationException($"Dimension '{dimension.Id}' is already registered");

        _dimensions[dimension.Id] = dimension;
        Logger.Info($"Registered dimension: {dimension.Id}");
    }

    /// <summary>
    /// Register multiple dimensions
    /// </summary>
    public void RegisterAll(params Dimension[] dimensions)
    {
        foreach (var dimension in dimensions)
        {
            Register(dimension);
        }
    }

    /// <summary>
    /// Get a dimension by its ID
    /// </summary>
    public Dimension? Get(ResourceKey id)
    {
        return _dimensions.TryGetValue(id, out var dimension) ? dimension : null;
    }

    /// <summary>
    /// Get a dimension by string ID
    /// </summary>
    public Dimension? Get(string id)
    {
        if (ResourceKey.TryParse(id, out var key))
        {
            return Get(key);
        }
        return null;
    }

    /// <summary>
    /// Check if a dimension is registered
    /// </summary>
    public bool IsRegistered(ResourceKey id)
    {
        return _dimensions.ContainsKey(id);
    }

    /// <summary>
    /// Get all registered dimensions
    /// </summary>
    public IReadOnlyCollection<Dimension> GetAllDimensions()
    {
        return _dimensions.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get dimensions by type
    /// </summary>
    public IEnumerable<Dimension> GetDimensionsByType(DimensionType type)
    {
        return _dimensions.Values.Where(d => d.Type == type);
    }

    /// <summary>
    /// Get all dimensions in a specific namespace
    /// </summary>
    public IEnumerable<Dimension> GetDimensionsInNamespace(string namespaceName)
    {
        return _dimensions.Values.Where(d => d.Id.IsInNamespace(namespaceName));
    }

    /// <summary>
    /// Unregister a dimension
    /// </summary>
    public bool Unregister(ResourceKey id)
    {
        if (_dimensions.TryGetValue(id, out var dimension) && dimension == DefaultDimension)
        {
            Logger.Warning($"Cannot unregister default dimension '{id}'");
            return false;
        }

        return _dimensions.Remove(id);
    }

    /// <summary>
    /// Update all dimensions
    /// </summary>
    public void UpdateAll(float deltaTime)
    {
        foreach (var dimension in _dimensions.Values)
        {
            try
            {
                dimension.OnUpdate(deltaTime);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating dimension '{dimension.Id}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clear all custom dimensions (keeps default dimensions)
    /// </summary>
    public void ClearCustomDimensions()
    {
        var toRemove = _dimensions.Where(kvp => 
            kvp.Value.Type == DimensionType.Custom &&
            kvp.Value != DefaultDimension
        ).Select(kvp => kvp.Key).ToList();

        foreach (var key in toRemove)
        {
            _dimensions.Remove(key);
        }

        Logger.Info($"Cleared {toRemove.Count} custom dimensions");
    }

    /// <summary>
    /// Get count of registered dimensions
    /// </summary>
    public int Count => _dimensions.Count;
}
