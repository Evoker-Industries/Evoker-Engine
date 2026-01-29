---
tags:
  - gamesystem
---

# Block System

The Block system in Evoker Engine provides a flexible framework for creating voxel-based blocks with properties, states, and behaviors similar to Minecraft.

## Overview

The block system includes:

- **Blocks**: Base objects with properties like hardness, light level, and transparency
- **Block States**: Dynamic properties (facing direction, powered state, etc.)
- **Block Registry**: Central management for all block types
- **Block Events**: Callbacks for placement, breaking, interaction, and updates
- **Drop System**: Configurable item drops when blocks are broken

## Block Class

```csharp
public abstract class Block
{
    public ResourceKey Id { get; set; }              // namespace:key identifier
    public string Name { get; set; }                 // Display name
    public float Hardness { get; set; }              // Mining time multiplier
    public float Resistance { get; set; }            // Explosion resistance
    public int LightLevel { get; set; }              // Light emitted (0-15)
    public bool IsSolid { get; set; }                // Blocks movement
    public bool IsTransparent { get; set; }          // Light passes through
    public bool IsBreakable { get; set; }            // Can be destroyed
    public string ModelId { get; set; }              // Texture/model ID
    public Dictionary<string, object> Properties { get; set; }
    public Dictionary<string, string> States { get; set; }
    
    public virtual void OnPlaced(Vector3 position, object? context = null);
    public virtual void OnBroken(Vector3 position, object? context = null);
    public virtual bool OnInteract(Vector3 position, object? context = null);
    public virtual void OnUpdate(Vector3 position, float deltaTime);
    public virtual List<(ResourceKey itemId, int quantity)> GetDrops(object? context = null);
}
```

## Creating Blocks

### Simple Block

```csharp
using EvokerEngine.Blocks;
using EvokerEngine.Core;

public class StoneBlock : Block
{
    public StoneBlock()
    {
        Id = new ResourceKey("game", "stone");
        Name = "Stone";
        Hardness = 1.5f;
        Resistance = 6.0f;
        IsSolid = true;
        IsTransparent = false;
        ModelId = "block/stone";
    }
}
```

### Block with Light

```csharp
public class TorchBlock : Block
{
    public TorchBlock()
    {
        Id = new ResourceKey("game", "torch");
        Name = "Torch";
        Hardness = 0f;  // Instantly breakable
        Resistance = 0f;
        IsSolid = false;
        IsTransparent = true;
        LightLevel = 14;  // Very bright
        ModelId = "block/torch";
    }
}
```

### Block with States

```csharp
public class DoorBlock : Block
{
    public DoorBlock()
    {
        Id = new ResourceKey("game", "door");
        Name = "Wooden Door";
        Hardness = 3.0f;
        IsSolid = true;
        
        // Initial states
        States["open"] = "false";
        States["facing"] = "north";
        States["half"] = "lower";
    }

    public override bool OnInteract(Vector3 position, object? context = null)
    {
        // Toggle door state
        bool isOpen = States["open"] == "true";
        States["open"] = (!isOpen).ToString().ToLower();
        
        Logger.Info($"Door {(isOpen ? "closed" : "opened")}");
        return true;
    }
}
```

### Block with Custom Behavior

```csharp
public class TNTBlock : Block
{
    public TNTBlock()
    {
        Id = new ResourceKey("game", "tnt");
        Name = "TNT";
        Hardness = 0f;
        Resistance = 0f;  // Weak to explosions
    }

    public override void OnBroken(Vector3 position, object? context = null)
    {
        // Explode when broken
        Explode(position, radius: 4f);
    }

    public override bool OnInteract(Vector3 position, object? context = null)
    {
        // Ignite with flint and steel
        StartTimer(position, fuseDuration: 4f);
        return true;
    }

    private void Explode(Vector3 position, float radius)
    {
        Logger.Info($"TNT exploded at {position}!");
        // Explosion logic...
    }

    private void StartTimer(Vector3 position, float fuseDuration)
    {
        Logger.Info("TNT fuse lit!");
        // Timer logic...
    }
}
```

### Block with Custom Drops

```csharp
public class OreBlock : Block
{
    public ResourceKey DropItemId { get; set; }
    public int MinDrops { get; set; } = 1;
    public int MaxDrops { get; set; } = 1;

    public OreBlock(string name, ResourceKey dropItem)
    {
        Id = new ResourceKey("game", name.ToLower().Replace(" ", "_"));
        Name = name;
        Hardness = 3.0f;
        DropItemId = dropItem;
    }

    public override List<(ResourceKey itemId, int quantity)> GetDrops(object? context = null)
    {
        var amount = Random.Shared.Next(MinDrops, MaxDrops + 1);
        return new List<(ResourceKey, int)> { (DropItemId, amount) };
    }
}

// Usage
var diamondOre = new OreBlock("Diamond Ore", new ResourceKey("game", "diamond"))
{
    MinDrops = 1,
    MaxDrops = 1,
    Hardness = 3.0f
};
```

## Block Registry

Central management for all blocks.

```csharp
public class BlockRegistry
{
    public static BlockRegistry Instance { get; }
    
    public void Register(Block block);
    public void RegisterAll(params Block[] blocks);
    public Block? Get(ResourceKey id);
    public Block? Get(string id);
    public IEnumerable<Block> GetAll();
    public bool IsRegistered(ResourceKey id);
}
```

### Registering Blocks

```csharp
using EvokerEngine.Blocks;

// Register single block
var stone = new StoneBlock();
BlockRegistry.Instance.Register(stone);

// Register multiple blocks
BlockRegistry.Instance.RegisterAll(
    new StoneBlock(),
    new DirtBlock(),
    new GrassBlock(),
    new TorchBlock()
);

// Get registered block
var block = BlockRegistry.Instance.Get("game:stone");
if (block != null)
{
    Logger.Info($"Found block: {block.Name}");
}
```

## Block Properties

### Custom Properties

```csharp
var furnace = new Block
{
    Id = new ResourceKey("game", "furnace"),
    Name = "Furnace"
};

// Add custom properties
furnace.Properties["cookTime"] = 10f;
furnace.Properties["fuelEfficiency"] = 1.5f;
furnace.Properties["maxHeat"] = 1000;

// Access properties
if (furnace.Properties.TryGetValue("cookTime", out var cookTime))
{
    float time = (float)cookTime;
}
```

## Block States

Dynamic properties that can change at runtime.

```csharp
// Set initial states
var lever = new Block
{
    Id = new ResourceKey("game", "lever"),
    Name = "Lever"
};
lever.States["powered"] = "false";
lever.States["facing"] = "north";

// Change state
lever.States["powered"] = "true";

// Query state
bool isPowered = lever.States.TryGetValue("powered", out var state) && 
                 state == "true";
```

## Complete Examples

### Example 1: Farmland System

```csharp
public class FarmlandBlock : Block
{
    public FarmlandBlock()
    {
        Id = new ResourceKey("game", "farmland");
        Name = "Farmland";
        Hardness = 0.6f;
        IsSolid = true;
        
        States["moisture"] = "0";  // 0-7, 7 = fully wet
    }

    public override void OnUpdate(Vector3 position, float deltaTime)
    {
        // Dry out over time if no water nearby
        int moisture = int.Parse(States["moisture"]);
        if (moisture > 0 && !IsWaterNearby(position))
        {
            moisture--;
            States["moisture"] = moisture.ToString();
            
            if (moisture == 0)
            {
                // Turn back to dirt
                ConvertToDirt(position);
            }
        }
    }

    public override void OnPlaced(Vector3 position, object? context = null)
    {
        // Check for water nearby
        if (IsWaterNearby(position))
        {
            States["moisture"] = "7";
        }
    }

    private bool IsWaterNearby(Vector3 position)
    {
        // Check 4-block radius for water
        // Implementation...
        return false;
    }

    private void ConvertToDirt(Vector3 position)
    {
        Logger.Info("Farmland dried out, converting to dirt");
        // Replace block...
    }
}
```

### Example 2: Redstone-like System

```csharp
public class RedstoneLampBlock : Block
{
    public RedstoneLampBlock()
    {
        Id = new ResourceKey("game", "redstone_lamp");
        Name = "Redstone Lamp";
        States["lit"] = "false";
    }

    public void SetPowered(bool powered)
    {
        bool wasLit = States["lit"] == "true";
        States["lit"] = powered.ToString().ToLower();
        
        if (powered != wasLit)
        {
            LightLevel = powered ? 15 : 0;
            Logger.Info($"Lamp {(powered ? "on" : "off")}");
        }
    }
}

public class LeverBlock : Block
{
    public LeverBlock()
    {
        Id = new ResourceKey("game", "lever");
        Name = "Lever";
        States["powered"] = "false";
    }

    public override bool OnInteract(Vector3 position, object? context = null)
    {
        bool isPowered = States["powered"] == "true";
        States["powered"] = (!isPowered).ToString().ToLower();
        
        // Notify connected blocks
        UpdateConnectedBlocks(position, !isPowered);
        return true;
    }

    private void UpdateConnectedBlocks(Vector3 position, bool powered)
    {
        // Find and update connected redstone lamps
        // Implementation...
    }
}
```

### Example 3: Growth System

```csharp
public class CropBlock : Block
{
    public int MaxGrowthStage { get; set; } = 7;
    public float GrowthTime { get; set; } = 60f;  // seconds

    public CropBlock(string cropName)
    {
        Id = new ResourceKey("game", cropName);
        Name = cropName;
        IsSolid = false;
        IsTransparent = true;
        
        States["age"] = "0";
    }

    public override void OnUpdate(Vector3 position, float deltaTime)
    {
        int age = int.Parse(States["age"]);
        
        if (age < MaxGrowthStage)
        {
            // Random growth chance
            if (Random.Shared.NextDouble() < deltaTime / GrowthTime)
            {
                age++;
                States["age"] = age.ToString();
                
                if (age == MaxGrowthStage)
                {
                    Logger.Info($"{Name} fully grown!");
                }
            }
        }
    }

    public override List<(ResourceKey itemId, int quantity)> GetDrops(object? context = null)
    {
        int age = int.Parse(States["age"]);
        
        if (age == MaxGrowthStage)
        {
            // Fully grown - drop crops
            return new List<(ResourceKey, int)>
            {
                (new ResourceKey("game", $"{Id.Key}_item"), Random.Shared.Next(2, 5)),
                (new ResourceKey("game", $"{Id.Key}_seeds"), Random.Shared.Next(1, 3))
            };
        }
        else
        {
            // Not grown - just drop seeds
            return new List<(ResourceKey, int)>
            {
                (new ResourceKey("game", $"{Id.Key}_seeds"), 1)
            };
        }
    }
}
```

## Best Practices

### ✅ Do's

- Use ResourceKey format (namespace:key) for block IDs
- Set appropriate hardness and resistance values
- Use block states for dynamic properties
- Implement GetDrops() for custom loot
- Call base methods when overriding

### ❌ Don'ts

- Don't use spaces or special characters in IDs
- Don't make all blocks indestructible (IsBreakable = false)
- Don't forget to register blocks before using them
- Don't perform heavy computation in OnUpdate()

## See Also

- [Inventory](inventory.md) - Item system for drops
- [Crafting](crafting.md) - Block crafting recipes
- [ResourceKey](../core/application.md) - ID system
