using System;
using System.Collections.Generic;
using System.Linq;
using EvokerEngine.Core;

namespace EvokerEngine.Crafting;

/// <summary>
/// Represents an ingredient in a recipe
/// </summary>
public class Ingredient
{
    /// <summary>
    /// Item or block ID
    /// </summary>
    public ResourceKey ItemId { get; set; }

    /// <summary>
    /// Required quantity
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Optional: specific item properties that must match
    /// </summary>
    public Dictionary<string, object>? RequiredProperties { get; set; }

    public Ingredient(ResourceKey itemId, int quantity = 1)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    public Ingredient(string itemId, int quantity = 1) 
        : this(ResourceKey.Parse(itemId), quantity)
    {
    }
}

/// <summary>
/// Represents the result of a recipe
/// </summary>
public class RecipeResult
{
    /// <summary>
    /// Item or block ID
    /// </summary>
    public ResourceKey ItemId { get; set; }

    /// <summary>
    /// Quantity produced
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Optional: custom properties for the result item
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }

    public RecipeResult(ResourceKey itemId, int quantity = 1)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    public RecipeResult(string itemId, int quantity = 1)
        : this(ResourceKey.Parse(itemId), quantity)
    {
    }
}

/// <summary>
/// Base class for all recipe types
/// </summary>
public abstract class Recipe
{
    /// <summary>
    /// Unique identifier for this recipe
    /// </summary>
    public ResourceKey Id { get; set; }

    /// <summary>
    /// Recipe type identifier (e.g., "evoker:crafting", "evoker:smelting")
    /// </summary>
    public ResourceKey Type { get; set; }

    /// <summary>
    /// Display name of the recipe
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Input ingredients
    /// </summary>
    public List<Ingredient> Ingredients { get; set; } = new();

    /// <summary>
    /// Output result
    /// </summary>
    public RecipeResult Result { get; set; } = null!;

    /// <summary>
    /// Custom data for recipe type-specific information
    /// </summary>
    public Dictionary<string, object> CustomData { get; set; } = new();

    /// <summary>
    /// Check if the recipe can be crafted with given items
    /// </summary>
    public abstract bool CanCraft(Dictionary<ResourceKey, int> availableItems);

    /// <summary>
    /// Perform the crafting operation
    /// </summary>
    public abstract RecipeResult Craft(Dictionary<ResourceKey, int> availableItems);

    /// <summary>
    /// Get consumed items for this recipe
    /// </summary>
    public virtual Dictionary<ResourceKey, int> GetConsumedItems()
    {
        return Ingredients.ToDictionary(i => i.ItemId, i => i.Quantity);
    }

    public override string ToString()
    {
        return $"{Name} ({Id}) - Type: {Type}";
    }
}

/// <summary>
/// Shapeless crafting recipe (order doesn't matter)
/// </summary>
public class ShapelessRecipe : Recipe
{
    public ShapelessRecipe()
    {
        Type = new ResourceKey("evoker", "shapeless_crafting");
    }

    public override bool CanCraft(Dictionary<ResourceKey, int> availableItems)
    {
        foreach (var ingredient in Ingredients)
        {
            if (!availableItems.TryGetValue(ingredient.ItemId, out var available) || 
                available < ingredient.Quantity)
            {
                return false;
            }
        }
        return true;
    }

    public override RecipeResult Craft(Dictionary<ResourceKey, int> availableItems)
    {
        if (!CanCraft(availableItems))
            throw new InvalidOperationException("Cannot craft: insufficient ingredients");

        // Consume ingredients
        foreach (var ingredient in Ingredients)
        {
            availableItems[ingredient.ItemId] -= ingredient.Quantity;
        }

        return Result;
    }
}

/// <summary>
/// Shaped crafting recipe (pattern-based, like a 3x3 grid)
/// </summary>
public class ShapedRecipe : Recipe
{
    /// <summary>
    /// Pattern definition (3x3 grid)
    /// Example: ["AAA", "BCB", "AAA"] where A, B, C are ingredient keys
    /// </summary>
    public List<string> Pattern { get; set; } = new();

    /// <summary>
    /// Mapping of pattern keys to ingredients
    /// Example: { "A": stone, "B": stick, "C": diamond }
    /// </summary>
    public Dictionary<char, Ingredient> PatternKey { get; set; } = new();

    public ShapedRecipe()
    {
        Type = new ResourceKey("evoker", "shaped_crafting");
    }

    public override bool CanCraft(Dictionary<ResourceKey, int> availableItems)
    {
        var required = new Dictionary<ResourceKey, int>();

        // Count required items from pattern
        foreach (var row in Pattern)
        {
            foreach (var ch in row)
            {
                if (ch == ' ' || ch == '_') continue; // Empty slot

                if (!PatternKey.TryGetValue(ch, out var ingredient))
                    return false;

                if (!required.ContainsKey(ingredient.ItemId))
                    required[ingredient.ItemId] = 0;
                
                required[ingredient.ItemId] += ingredient.Quantity;
            }
        }

        // Check if we have enough of each required item
        foreach (var (itemId, quantity) in required)
        {
            if (!availableItems.TryGetValue(itemId, out var available) || available < quantity)
                return false;
        }

        return true;
    }

    public override RecipeResult Craft(Dictionary<ResourceKey, int> availableItems)
    {
        if (!CanCraft(availableItems))
            throw new InvalidOperationException("Cannot craft: pattern doesn't match or insufficient ingredients");

        // Consume items based on pattern
        var consumed = new Dictionary<ResourceKey, int>();
        foreach (var row in Pattern)
        {
            foreach (var ch in row)
            {
                if (ch == ' ' || ch == '_') continue;

                if (PatternKey.TryGetValue(ch, out var ingredient))
                {
                    if (!consumed.ContainsKey(ingredient.ItemId))
                        consumed[ingredient.ItemId] = 0;
                    consumed[ingredient.ItemId] += ingredient.Quantity;
                }
            }
        }

        foreach (var (itemId, quantity) in consumed)
        {
            availableItems[itemId] -= quantity;
        }

        return Result;
    }
}

/// <summary>
/// Smelting/furnace recipe
/// </summary>
public class SmeltingRecipe : Recipe
{
    /// <summary>
    /// Time required to smelt (in seconds)
    /// </summary>
    public float SmeltTime { get; set; } = 10f;

    /// <summary>
    /// Experience gained when smelting
    /// </summary>
    public float Experience { get; set; } = 0.1f;

    public SmeltingRecipe()
    {
        Type = new ResourceKey("evoker", "smelting");
    }

    public override bool CanCraft(Dictionary<ResourceKey, int> availableItems)
    {
        if (Ingredients.Count != 1)
            return false;

        var ingredient = Ingredients[0];
        return availableItems.TryGetValue(ingredient.ItemId, out var available) && 
               available >= ingredient.Quantity;
    }

    public override RecipeResult Craft(Dictionary<ResourceKey, int> availableItems)
    {
        if (!CanCraft(availableItems))
            throw new InvalidOperationException("Cannot smelt: insufficient ingredients");

        var ingredient = Ingredients[0];
        availableItems[ingredient.ItemId] -= ingredient.Quantity;

        return Result;
    }
}
