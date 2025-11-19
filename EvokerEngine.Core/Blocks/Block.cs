using System;
using System.Collections.Generic;
using System.Numerics;
using EvokerEngine.Core;

namespace EvokerEngine.Blocks;

/// <summary>
/// Base class for all blocks in the game
/// </summary>
public abstract class Block
{
    /// <summary>
    /// Unique identifier for this block using namespace:key format
    /// </summary>
    public ResourceKey Id { get; set; }

    /// <summary>
    /// Display name of the block
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Block hardness (mining time multiplier)
    /// </summary>
    public float Hardness { get; set; } = 1.0f;

    /// <summary>
    /// Block resistance to explosions
    /// </summary>
    public float Resistance { get; set; } = 1.0f;

    /// <summary>
    /// Light level emitted by this block (0-15)
    /// </summary>
    public int LightLevel { get; set; } = 0;

    /// <summary>
    /// Whether the block is solid (blocks movement)
    /// </summary>
    public bool IsSolid { get; set; } = true;

    /// <summary>
    /// Whether the block is transparent (light passes through)
    /// </summary>
    public bool IsTransparent { get; set; } = false;

    /// <summary>
    /// Whether the block can be broken/destroyed
    /// </summary>
    public bool IsBreakable { get; set; } = true;

    /// <summary>
    /// Texture/model ID for rendering
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Custom properties for extensibility
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Block states (e.g., "facing": "north", "powered": "true")
    /// </summary>
    public Dictionary<string, string> States { get; set; } = new();

    /// <summary>
    /// Called when the block is placed in the world
    /// </summary>
    public virtual void OnPlaced(Vector3 position, object? context = null)
    {
    }

    /// <summary>
    /// Called when the block is broken/destroyed
    /// </summary>
    public virtual void OnBroken(Vector3 position, object? context = null)
    {
    }

    /// <summary>
    /// Called when an entity interacts with the block
    /// </summary>
    public virtual bool OnInteract(Vector3 position, object? context = null)
    {
        return false;
    }

    /// <summary>
    /// Called every tick for blocks that need updates
    /// </summary>
    public virtual void OnUpdate(Vector3 position, float deltaTime)
    {
    }

    /// <summary>
    /// Get drops when block is broken
    /// </summary>
    public virtual List<(ResourceKey itemId, int quantity)> GetDrops(object? context = null)
    {
        // By default, drop the block itself
        return new List<(ResourceKey, int)> { (Id, 1) };
    }

    /// <summary>
    /// Clone this block
    /// </summary>
    public virtual Block Clone()
    {
        var clone = (Block)MemberwiseClone();
        clone.Properties = new Dictionary<string, object>(Properties);
        clone.States = new Dictionary<string, string>(States);
        return clone;
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}

/// <summary>
/// Simple block implementation for basic blocks
/// </summary>
public class SimpleBlock : Block
{
    public SimpleBlock(ResourceKey id, string name)
    {
        Id = id;
        Name = name;
    }

    public SimpleBlock(string namespacePart, string key, string name) 
        : this(new ResourceKey(namespacePart, key), name)
    {
    }
}
