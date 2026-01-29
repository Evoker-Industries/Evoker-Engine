---
tags:
  - gamesystem
---

# World Serialization

The Evoker Engine provides a comprehensive world serialization system for saving and loading game worlds, including blocks, chunks, player data, and custom metadata.

## Overview

The serialization system consists of:

- **WorldData**: Container for all world information
- **WorldSerializer**: Low-level save/load operations
- **WorldManager**: High-level world management
- **ChunkData**: Chunk information with block data
- **PlayerData**: Player state and inventory
- **Compression**: Optional GZIP compression for smaller file sizes

## Quick Start

### Basic World Saving

```csharp
using EvokerEngine.Serialization;

// Create world data
var worldData = new WorldData
{
    Name = "My World",
    Seed = 12345,
    SpawnPosition = new Vector3Data(0, 64, 0)
};

// Add chunk data
var chunk = new ChunkData { X = 0, Z = 0 };
chunk.Blocks.Add(new BlockData
{
    X = 5,
    Y = 10,
    Z = 5,
    Type = 1  // Grass block
});
worldData.Chunks.Add(chunk);

// Save world
WorldSerializer.SaveWorld(worldData, "worlds/myworld.world", compress: true);
```

### Loading a World

```csharp
// Load world
var loadedWorld = WorldSerializer.LoadWorld("worlds/myworld.world", compressed: true);

if (loadedWorld != null)
{
    Console.WriteLine($"Loaded: {loadedWorld.Name}");
    Console.WriteLine($"Seed: {loadedWorld.Seed}");
    Console.WriteLine($"Chunks: {loadedWorld.Chunks.Count}");
}
```

## Using WorldManager

The `WorldManager` class provides a higher-level API for world management:

```csharp
// Create manager
var manager = new WorldManager("./worlds");

// Create new world
var worldData = manager.CreateNewWorld("Adventure World", seed: 42);

// Save world
manager.SaveWorld(worldData, "Adventure World");

// Load world
var loadedWorld = manager.LoadWorld("Adventure World");

// Check if world exists
if (manager.WorldExists("Adventure World"))
{
    Console.WriteLine("World found!");
}

// List all worlds
var worlds = manager.ListWorlds();
foreach (var worldName in worlds)
{
    Console.WriteLine($"Found world: {worldName}");
}

// Get metadata without loading entire world
var metadata = manager.GetWorldMetadata("Adventure World");
if (metadata != null)
{
    Console.WriteLine($"Name: {metadata.Name}");
    Console.WriteLine($"Seed: {metadata.Seed}");
    Console.WriteLine($"Created: {metadata.CreatedAt}");
    Console.WriteLine($"Size: {metadata.GetFileSizeFormatted()}");
}

// Delete world
manager.DeleteWorld("Old World");
```

## WorldData Structure

### Basic Properties

```csharp
var worldData = new WorldData
{
    Name = "My World",
    Seed = 12345,
    Version = "1.0.0",
    GameTime = 1000,  // Game ticks
    SpawnPosition = new Vector3Data(0, 64, 0)
};

// Add custom metadata
worldData.Metadata["difficulty"] = "hard";
worldData.Metadata["gamemode"] = "survival";
worldData.Metadata["customRule"] = "value";
```

### Adding Chunks

```csharp
// Create a chunk
var chunk = new ChunkData
{
    X = 0,  // Chunk X coordinate
    Z = 0   // Chunk Z coordinate
};

// Add blocks to chunk
for (int x = 0; x < 16; x++)
{
    for (int z = 0; z < 16; z++)
    {
        chunk.Blocks.Add(new BlockData
        {
            X = x,
            Y = 64,
            Z = z,
            Type = 1,  // Grass
            Metadata = new Dictionary<string, string>
            {
                { "variant", "snowy" }
            }
        });
    }
}

worldData.Chunks.Add(chunk);
```

### Player Data

```csharp
worldData.Player = new PlayerData
{
    Name = "Steve",
    Position = new Vector3Data(10, 65, 10),
    Rotation = new Vector3Data(0, 0, 0),
    Health = 100f
};

// Add inventory items
worldData.Player.Inventory = new List<ItemData>
{
    new ItemData
    {
        Type = "minecraft:diamond_pickaxe",
        Count = 1,
        Metadata = new Dictionary<string, string>
        {
            { "durability", "1561" }
        }
    },
    new ItemData
    {
        Type = "minecraft:bread",
        Count = 64
    }
};

// Custom player data
worldData.Player.CustomData = new Dictionary<string, string>
{
    { "experience", "1000" },
    { "level", "30" }
};
```

## Compression

Worlds can be saved with or without compression:

```csharp
// Save with compression (recommended)
WorldSerializer.SaveWorld(worldData, "world.world", compress: true);

// Save without compression (useful for debugging)
WorldSerializer.SaveWorld(worldData, "world.json", compress: false);

// Load with compression
var compressed = WorldSerializer.LoadWorld("world.world", compressed: true);

// Load without compression
var uncompressed = WorldSerializer.LoadWorld("world.json", compressed: false);
```

Compression typically reduces file size by 70-90% depending on the world's complexity.

## Network Serialization

For multiplayer or custom storage:

```csharp
// Serialize to bytes
byte[] worldBytes = WorldSerializer.SerializeWorldToBytes(worldData, compress: true);

// Send over network or store in database
// ...

// Deserialize from bytes
var worldData = WorldSerializer.DeserializeWorldFromBytes(worldBytes, compressed: true);
```

## Advanced Usage

### Partial World Loading

```csharp
// Get metadata without loading entire world
var manager = new WorldManager();
var metadata = manager.GetWorldMetadata("Large World");

if (metadata != null)
{
    Console.WriteLine($"World: {metadata.Name}");
    Console.WriteLine($"Size: {metadata.GetFileSizeFormatted()}");
    Console.WriteLine($"Last saved: {metadata.LastSaved}");
    
    // Decide whether to load based on metadata
    if (metadata.FileSize < 10 * 1024 * 1024) // Less than 10 MB
    {
        var world = manager.LoadWorld("Large World");
    }
}
```

### Incremental Saving

For large worlds, you can save chunks individually:

```csharp
public class IncrementalWorldSaver
{
    private WorldData _worldData;
    private WorldManager _manager;
    
    public IncrementalWorldSaver(WorldData worldData)
    {
        _worldData = worldData;
        _manager = new WorldManager();
    }
    
    public void SaveDirtyChunks(HashSet<(int x, int z)> dirtyChunks)
    {
        // Only save modified chunks
        var chunksToSave = _worldData.Chunks
            .Where(c => dirtyChunks.Contains((c.X, c.Z)))
            .ToList();
        
        // Create partial world data with only dirty chunks
        var partialWorld = new WorldData
        {
            Name = _worldData.Name,
            Seed = _worldData.Seed,
            Chunks = chunksToSave,
            Player = _worldData.Player
        };
        
        _manager.SaveWorld(partialWorld, $"{_worldData.Name}_temp");
        
        // After verification, merge with main save
    }
}
```

### Custom Block Metadata

```csharp
var chest = new BlockData
{
    X = 10,
    Y = 64,
    Z = 10,
    Type = 54,  // Chest
    Metadata = new Dictionary<string, string>
    {
        { "inventory", JsonSerializer.Serialize(chestInventory) },
        { "locked", "true" },
        { "lockCode", "secret123" }
    }
};
```

### Versioning

Handle different world versions:

```csharp
public WorldData LoadWorldWithMigration(string worldName)
{
    var world = _manager.LoadWorld(worldName);
    
    if (world == null)
        return null;
    
    // Migrate from older versions
    switch (world.Version)
    {
        case "1.0.0":
            MigrateFrom1_0_0(world);
            world.Version = "1.1.0";
            break;
        case "1.1.0":
            MigrateFrom1_1_0(world);
            world.Version = "2.0.0";
            break;
    }
    
    // Save migrated world
    _manager.SaveWorld(world, worldName);
    
    return world;
}
```

## Best Practices

### 1. Auto-Save

```csharp
public class AutoSaveSystem
{
    private WorldManager _manager;
    private WorldData _currentWorld;
    private float _saveTimer = 0f;
    private const float SaveInterval = 300f; // 5 minutes
    
    public void Update(float deltaTime)
    {
        _saveTimer += deltaTime;
        
        if (_saveTimer >= SaveInterval)
        {
            SaveWorld();
            _saveTimer = 0f;
        }
    }
    
    private void SaveWorld()
    {
        _manager.SaveWorld(_currentWorld, _currentWorld.Name);
        Logger.Info("Auto-saved world");
    }
}
```

### 2. Backup System

```csharp
public void CreateBackup(string worldName)
{
    var world = _manager.LoadWorld(worldName);
    if (world != null)
    {
        string backupName = $"{worldName}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
        _manager.SaveWorld(world, backupName);
        Logger.Info($"Created backup: {backupName}");
    }
}
```

### 3. Validation

```csharp
public bool ValidateWorld(WorldData world)
{
    if (string.IsNullOrEmpty(world.Name))
    {
        Logger.Error("World has no name");
        return false;
    }
    
    if (world.Chunks == null)
    {
        Logger.Error("World has no chunks");
        return false;
    }
    
    // Check for corrupt chunks
    foreach (var chunk in world.Chunks)
    {
        if (chunk.Blocks == null)
        {
            Logger.Error($"Chunk ({chunk.X}, {chunk.Z}) has no blocks");
            return false;
        }
    }
    
    return true;
}
```

### 4. Error Handling

```csharp
public void SafeSaveWorld(WorldData world, string name)
{
    try
    {
        // Validate before saving
        if (!ValidateWorld(world))
        {
            Logger.Error("World validation failed");
            return;
        }
        
        // Create backup of existing world
        if (_manager.WorldExists(name))
        {
            CreateBackup(name);
        }
        
        // Save world
        _manager.SaveWorld(world, name);
        
        Logger.Info($"Successfully saved world: {name}");
    }
    catch (Exception ex)
    {
        Logger.Error($"Failed to save world: {ex.Message}");
        
        // Attempt to restore from backup if available
        // ...
    }
}
```

## File Format

World files use JSON format with optional GZIP compression:

```json
{
  "name": "My World",
  "seed": 12345,
  "createdAt": "2025-11-21T16:00:00Z",
  "lastSaved": "2025-11-21T17:30:00Z",
  "version": "1.0.0",
  "spawnPosition": {
    "x": 0,
    "y": 64,
    "z": 0
  },
  "gameTime": 1000,
  "metadata": {
    "difficulty": "hard",
    "gamemode": "survival"
  },
  "chunks": [
    {
      "x": 0,
      "z": 0,
      "blocks": [
        {
          "x": 5,
          "y": 10,
          "z": 5,
          "type": 1
        }
      ]
    }
  ],
  "player": {
    "name": "Player",
    "position": { "x": 10, "y": 65, "z": 10 },
    "rotation": { "x": 0, "y": 0, "z": 0 },
    "health": 100
  }
}
```

## Performance Considerations

1. **Compression**: Always use compression for production; it significantly reduces file size
2. **Incremental Saves**: For large worlds, only save modified chunks
3. **Async Operations**: Consider async I/O for large world files
4. **Chunk Limits**: Break very large worlds into multiple files if needed
5. **Memory**: Use streaming for extremely large worlds

## See Also

- [Block System](../systems/blocks.md)
- [Inventory System](../systems/inventory.md)
- [3D Games Tutorial](../game-dev/3d-games.md)
- [Networking System](../api/reference.md)
