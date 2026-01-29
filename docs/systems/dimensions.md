---
tags:
  - gamesystem
  - world
  - voxel
---

# Dimension System

The Dimension system in Evoker Engine provides a framework for creating multiple worlds/dimensions with different properties, similar to Minecraft's Overworld, Nether, and End dimensions.

## Overview

The dimension system includes:

- **Dimensions**: Separate worlds with unique properties
- **Dimension Types**: Overworld, Nether, End, and Custom
- **Dimension Registry**: Central management for all dimensions
- **Dimension Properties**: Height limits, gravity, ambient light, environment settings
- **Coordinate Conversion**: Between dimensions with different scales

## Dimension Class

```csharp
public class Dimension
{
    public ResourceKey Id { get; set; }
    public string Name { get; set; }
    public DimensionType Type { get; set; }
    
    // Lighting
    public int AmbientLight { get; set; }         // 0-15
    public bool HasSky { get; set; }
    public bool HasCeiling { get; set; }
    public Vector3 FogColor { get; set; }
    public Vector3 SkyColor { get; set; }
    
    // World Properties
    public float CoordinateScale { get; set; }    // Relative to overworld
    public int MinHeight { get; set; }
    public int MaxHeight { get; set; }
    public int Height { get; }                    // Total height
    
    // Gameplay
    public bool BedsWork { get; set; }
    public bool RespawnAnchorsWork { get; set; }
    public bool WaterEvaporates { get; set; }
    public bool LavaSpreadsWildly { get; set; }
    public bool HasRaids { get; set; }
    public float Gravity { get; set; }            // Multiplier (1.0 = normal)
}
```

## Built-in Dimensions

### Overworld

```csharp
var overworld = new Dimension
{
    Id = new ResourceKey("evoker", "overworld"),
    Name = "Overworld",
    Type = DimensionType.Overworld,
    
    // Lighting
    AmbientLight = 15,
    HasSky = true,
    HasCeiling = false,
    SkyColor = new Vector3(0.5f, 0.7f, 1.0f),
    FogColor = new Vector3(0.8f, 0.8f, 1.0f),
    
    // World Properties
    CoordinateScale = 1.0f,
    MinHeight = -64,
    MaxHeight = 320,
    
    // Gameplay
    BedsWork = true,
    RespawnAnchorsWork = false,
    WaterEvaporates = false,
    LavaSpreadsWildly = false,
    HasRaids = true,
    Gravity = 1.0f
};
```

### Nether

```csharp
var nether = new Dimension
{
    Id = new ResourceKey("evoker", "the_nether"),
    Name = "The Nether",
    Type = DimensionType.Nether,
    
    // Lighting
    AmbientLight = 8,  // Dim
    HasSky = false,
    HasCeiling = true,
    SkyColor = new Vector3(0.2f, 0.0f, 0.0f),
    FogColor = new Vector3(0.3f, 0.0f, 0.0f),
    
    // World Properties
    CoordinateScale = 8.0f,  // 8:1 ratio with overworld
    MinHeight = 0,
    MaxHeight = 128,
    
    // Gameplay
    BedsWork = false,  // Beds explode!
    RespawnAnchorsWork = true,
    WaterEvaporates = true,
    LavaSpreadsWildly = true,
    HasRaids = false,
    Gravity = 1.0f
};
```

### The End

```csharp
var theEnd = new Dimension
{
    Id = new ResourceKey("evoker", "the_end"),
    Name = "The End",
    Type = DimensionType.End,
    
    // Lighting
    AmbientLight = 0,  // Very dark
    HasSky = true,
    HasCeiling = false,
    SkyColor = new Vector3(0.0f, 0.0f, 0.0f),  // Black
    FogColor = new Vector3(0.1f, 0.0f, 0.2f),  // Dark purple
    
    // World Properties
    CoordinateScale = 1.0f,
    MinHeight = 0,
    MaxHeight = 256,
    
    // Gameplay
    BedsWork = false,
    RespawnAnchorsWork = false,
    WaterEvaporates = false,
    LavaSpreadsWildly = false,
    HasRaids = false,
    Gravity = 0.8f  // Slightly lower gravity
};
```

## Custom Dimensions

### Moon Dimension

```csharp
var moon = new Dimension
{
    Id = new ResourceKey("game", "moon"),
    Name = "The Moon",
    Type = DimensionType.Custom,
    
    // Lighting
    AmbientLight = 3,
    HasSky = true,
    HasCeiling = false,
    SkyColor = new Vector3(0.0f, 0.0f, 0.05f),  // Very dark blue
    FogColor = new Vector3(0.1f, 0.1f, 0.15f),
    
    // World Properties
    CoordinateScale = 1.0f,
    MinHeight = 0,
    MaxHeight = 128,
    
    // Gameplay
    BedsWork = true,
    WaterEvaporates = true,  // No atmosphere
    Gravity = 0.16f  // Moon has ~16% of Earth's gravity
};
```

### Sky Islands

```csharp
var skyIslands = new Dimension
{
    Id = new ResourceKey("game", "sky_islands"),
    Name = "Sky Islands",
    Type = DimensionType.Custom,
    
    // Lighting
    AmbientLight = 15,
    HasSky = true,
    HasCeiling = false,
    SkyColor = new Vector3(0.3f, 0.6f, 1.0f),
    FogColor = new Vector3(0.9f, 0.95f, 1.0f),
    
    // World Properties
    CoordinateScale = 1.0f,
    MinHeight = 0,
    MaxHeight = 512,  // Extra tall for floating islands
    
    // Gameplay
    Gravity = 0.7f  // Slightly lower gravity for jump height
};
```

## Dimension Registry

```csharp
public class DimensionRegistry
{
    public static DimensionRegistry Instance { get; }
    
    public void Register(Dimension dimension);
    public void RegisterAll(params Dimension[] dimensions);
    public Dimension? Get(ResourceKey id);
    public Dimension? Get(string id);
    public IEnumerable<Dimension> GetAll();
    public bool IsRegistered(ResourceKey id);
}
```

### Registering Dimensions

```csharp
using EvokerEngine.World;

// Register built-in dimensions
DimensionRegistry.Instance.RegisterAll(
    overworld,
    nether,
    theEnd
);

// Register custom dimension
DimensionRegistry.Instance.Register(moon);

// Get dimension
var dim = DimensionRegistry.Instance.Get("evoker:overworld");
if (dim != null)
{
    Logger.Info($"Dimension: {dim.Name}, Height: {dim.Height}");
}
```

## Coordinate Conversion

Convert coordinates between dimensions with different scales:

```csharp
// Overworld to Nether (divide by 8)
var overworldPos = new Vector3(800, 64, 1600);
var netherPos = overworld.ConvertTo(nether, overworldPos);
// Result: (100, 64, 200)

// Nether to Overworld (multiply by 8)
var netherPos = new Vector3(100, 64, 200);
var overworldPos = nether.ConvertTo(overworld, netherPos);
// Result: (800, 64, 1600)
```

## Portal System Example

```csharp
public class PortalSystem
{
    private Dimension _overworld;
    private Dimension _nether;

    public void TeleportToNether(Entity entity, Vector3 position)
    {
        var netherPos = _overworld.ConvertTo(_nether, position);
        
        // Find or create portal in nether
        var portalPos = FindOrCreatePortal(netherPos);
        
        // Teleport entity
        TeleportEntity(entity, _nether, portalPos);
        
        Logger.Info($"Teleported to Nether at {portalPos}");
    }

    public void TeleportToOverworld(Entity entity, Vector3 position)
    {
        var overworldPos = _nether.ConvertTo(_overworld, position);
        
        // Find or create portal in overworld
        var portalPos = FindOrCreatePortal(overworldPos);
        
        // Teleport entity
        TeleportEntity(entity, _overworld, portalPos);
        
        Logger.Info($"Teleported to Overworld at {portalPos}");
    }

    private Vector3 FindOrCreatePortal(Vector3 targetPos)
    {
        // Search for existing portal within 128 blocks
        // If not found, create new portal
        return targetPos;
    }

    private void TeleportEntity(Entity entity, Dimension dimension, Vector3 position)
    {
        // Change entity's dimension and position
    }
}
```

## Dimension Properties Examples

### High Gravity World

```csharp
var highGravity = new Dimension
{
    Id = new ResourceKey("game", "high_gravity"),
    Name = "High Gravity World",
    Gravity = 2.0f,  // Double gravity
    MinHeight = -128,
    MaxHeight = 128  // Lower ceiling due to strong gravity
};
```

### No Gravity (Space)

```csharp
var space = new Dimension
{
    Id = new ResourceKey("game", "space"),
    Name = "Space",
    Gravity = 0.0f,  // Zero gravity
    AmbientLight = 0,
    HasSky = true,
    SkyColor = new Vector3(0, 0, 0),  // Black space
    WaterEvaporates = true  // No atmosphere
};
```

### Underground Dimension

```csharp
var underworld = new Dimension
{
    Id = new ResourceKey("game", "underworld"),
    Name = "Underworld",
    HasSky = false,
    HasCeiling = true,
    AmbientLight = 0,  // Completely dark
    MinHeight = -256,
    MaxHeight = 0,  // Entire dimension is underground
    LavaSpreadsWildly = true
};
```

## Complete Example

```csharp
public class DimensionManager : Layer
{
    private Dictionary<ResourceKey, Dimension> _dimensions = new();
    private Dimension _currentDimension;

    public override void OnAttach()
    {
        // Register all dimensions
        RegisterDimensions();
        
        // Set starting dimension
        _currentDimension = DimensionRegistry.Instance.Get("evoker:overworld")!;
    }

    private void RegisterDimensions()
    {
        // Built-in dimensions
        var overworld = CreateOverworld();
        var nether = CreateNether();
        var theEnd = CreateEnd();
        
        // Custom dimensions
        var moon = CreateMoon();
        var skylands = CreateSkylands();
        
        DimensionRegistry.Instance.RegisterAll(
            overworld, nether, theEnd, moon, skylands
        );
    }

    public void ChangeDimension(Entity entity, ResourceKey dimensionId)
    {
        var newDimension = DimensionRegistry.Instance.Get(dimensionId);
        if (newDimension == null)
        {
            Logger.Error($"Dimension not found: {dimensionId}");
            return;
        }
        
        var scene = Application.Instance.ActiveScene;
        var transform = scene.Registry.GetComponent<TransformComponent>(entity);
        if (transform != null)
        {
            // Convert coordinates if needed
            var newPos = _currentDimension.ConvertTo(newDimension, transform.Position);
            transform.Position = newPos;
            
            // Update current dimension
            _currentDimension = newDimension;
            
            // Apply dimension properties
            ApplyDimensionEffects(entity, newDimension);
            
            Logger.Info($"Changed to dimension: {newDimension.Name}");
        }
    }

    private void ApplyDimensionEffects(Entity entity, Dimension dimension)
    {
        // Apply gravity
        var scene = Application.Instance.ActiveScene;
        if (scene.Registry.HasComponent<RigidbodyComponent>(entity))
        {
            var rb = scene.Registry.GetComponent<RigidbodyComponent>(entity);
            rb!.GravityScale = dimension.Gravity;
        }
        
        // Apply other effects (fog color, ambient light, etc.)
    }

    private Dimension CreateOverworld() { /* ... */ return new Dimension(); }
    private Dimension CreateNether() { /* ... */ return new Dimension(); }
    private Dimension CreateEnd() { /* ... */ return new Dimension(); }
    private Dimension CreateMoon() { /* ... */ return new Dimension(); }
    private Dimension CreateSkylands() { /* ... */ return new Dimension(); }
}
```

## Best Practices

### ✅ Do's

- Use ResourceKey format for dimension IDs
- Set appropriate coordinate scales for dimension travel
- Balance gravity values for gameplay
- Use ambient light for atmosphere
- Register dimensions at startup

### ❌ Don'ts

- Don't create dimensions with extreme height limits (performance)
- Don't set gravity to negative values (unless intentional)
- Don't forget to convert coordinates when teleporting
- Don't make all custom dimensions with normal properties

## See Also

- [Blocks](blocks.md) - Different blocks for each dimension
- [World Generation](../game-dev/overview.md) - Generating terrain
