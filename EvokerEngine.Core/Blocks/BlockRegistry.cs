using System;
using System.Collections.Generic;
using System.Linq;
using EvokerEngine.Core;

namespace EvokerEngine.Blocks;

/// <summary>
/// Registry for managing all blocks in the game
/// </summary>
public class BlockRegistry
{
    private static BlockRegistry? _instance;
    private readonly Dictionary<ResourceKey, Block> _blocks = new();

    /// <summary>
    /// Singleton instance of the block registry
    /// </summary>
    public static BlockRegistry Instance => _instance ??= new BlockRegistry();

    /// <summary>
    /// Register a new block
    /// </summary>
    public void Register(Block block)
    {
        if (block == null)
            throw new ArgumentNullException(nameof(block));

        if (_blocks.ContainsKey(block.Id))
            throw new InvalidOperationException($"Block with ID '{block.Id}' is already registered");

        _blocks[block.Id] = block;
        Logger.Info($"Registered block: {block.Id}");
    }

    /// <summary>
    /// Register multiple blocks
    /// </summary>
    public void RegisterAll(params Block[] blocks)
    {
        foreach (var block in blocks)
        {
            Register(block);
        }
    }

    /// <summary>
    /// Get a block by its resource key
    /// </summary>
    public Block? Get(ResourceKey id)
    {
        return _blocks.TryGetValue(id, out var block) ? block : null;
    }

    /// <summary>
    /// Get a block by string ID (will be parsed as ResourceKey)
    /// </summary>
    public Block? Get(string id)
    {
        if (ResourceKey.TryParse(id, out var key))
        {
            return Get(key);
        }
        return null;
    }

    /// <summary>
    /// Check if a block is registered
    /// </summary>
    public bool IsRegistered(ResourceKey id)
    {
        return _blocks.ContainsKey(id);
    }

    /// <summary>
    /// Get all registered blocks
    /// </summary>
    public IReadOnlyCollection<Block> GetAllBlocks()
    {
        return _blocks.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get all blocks in a specific namespace
    /// </summary>
    public IEnumerable<Block> GetBlocksInNamespace(string namespaceName)
    {
        return _blocks.Values.Where(b => b.Id.IsInNamespace(namespaceName));
    }

    /// <summary>
    /// Unregister a block
    /// </summary>
    public bool Unregister(ResourceKey id)
    {
        return _blocks.Remove(id);
    }

    /// <summary>
    /// Clear all registered blocks
    /// </summary>
    public void Clear()
    {
        _blocks.Clear();
        Logger.Info("Cleared all registered blocks");
    }

    /// <summary>
    /// Get count of registered blocks
    /// </summary>
    public int Count => _blocks.Count;
}
