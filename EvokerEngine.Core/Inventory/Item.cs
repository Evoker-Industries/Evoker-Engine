using System;
using System.Collections.Generic;
using EvokerEngine.Core;

namespace EvokerEngine.Inventory;

/// <summary>
/// Base class for all items in the game
/// </summary>
public abstract class Item
{
    /// <summary>
    /// Unique identifier for this item type using namespace:key format
    /// </summary>
    public ResourceKey Id { get; set; }

    /// <summary>
    /// Display name of the item
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the item
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon or sprite ID for the item
    /// </summary>
    public string IconId { get; set; } = string.Empty;

    /// <summary>
    /// Maximum stack size for this item (1 = non-stackable)
    /// </summary>
    public int MaxStackSize { get; set; } = 1;

    /// <summary>
    /// Base value/price of the item
    /// </summary>
    public int Value { get; set; } = 0;

    /// <summary>
    /// Weight of a single item
    /// </summary>
    public float Weight { get; set; } = 0f;

    /// <summary>
    /// Item rarity level
    /// </summary>
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    /// <summary>
    /// Custom properties for extensibility
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; set; } = new();

    /// <summary>
    /// Called when the item is used
    /// </summary>
    public virtual bool OnUse(object? context = null)
    {
        return false; // Not consumed by default
    }

    /// <summary>
    /// Called when the item is equipped (for equipment items)
    /// </summary>
    public virtual void OnEquip(object? context = null)
    {
    }

    /// <summary>
    /// Called when the item is unequipped (for equipment items)
    /// </summary>
    public virtual void OnUnequip(object? context = null)
    {
    }

    /// <summary>
    /// Clone this item to create a new instance
    /// </summary>
    public virtual Item Clone()
    {
        var clone = (Item)MemberwiseClone();
        clone.CustomProperties = new Dictionary<string, object>(CustomProperties);
        return clone;
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}

/// <summary>
/// Item rarity levels
/// </summary>
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

/// <summary>
/// Simple item implementation
/// </summary>
public class SimpleItem : Item
{
    public SimpleItem(ResourceKey id, string name)
    {
        Id = id;
        Name = name;
    }

    public SimpleItem(string namespacePart, string key, string name)
        : this(new ResourceKey(namespacePart, key), name)
    {
    }
}
