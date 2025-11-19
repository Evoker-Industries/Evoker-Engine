using System;
using EvokerEngine.Blocks;
using EvokerEngine.Inventory;
using EvokerEngine.Crafting;
using EvokerEngine.World;
using EvokerEngine.Core;

namespace EvokerEngine.Modding;

/// <summary>
/// Helper API for mods to easily register content
/// </summary>
public class ModAPI
{
    private readonly Mod _mod;

    internal ModAPI(Mod mod)
    {
        _mod = mod;
    }

    /// <summary>
    /// Get a resource key for this mod
    /// </summary>
    public ResourceKey Key(string key) => new ResourceKey(_mod.Info.ModId, key);

    #region Blocks

    /// <summary>
    /// Register a block
    /// </summary>
    public void RegisterBlock(Block block)
    {
        BlockRegistry.Instance.Register(block);
    }

    /// <summary>
    /// Create and register a simple block
    /// </summary>
    public SimpleBlock RegisterSimpleBlock(string key, string name, Action<SimpleBlock>? configure = null)
    {
        var block = new SimpleBlock(Key(key), name);
        configure?.Invoke(block);
        RegisterBlock(block);
        return block;
    }

    #endregion

    #region Items

    /// <summary>
    /// Create a simple item with this mod's namespace
    /// </summary>
    public SimpleItem CreateSimpleItem(string key, string name)
    {
        return new SimpleItem(Key(key), name);
    }

    #endregion

    #region Recipes

    /// <summary>
    /// Register a recipe
    /// </summary>
    public void RegisterRecipe(Recipe recipe)
    {
        RecipeRegistry.Instance.Register(recipe);
    }

    /// <summary>
    /// Create and register a shapeless recipe
    /// </summary>
    public ShapelessRecipe RegisterShapelessRecipe(string recipeKey, Action<ShapelessRecipe> configure)
    {
        var recipe = new ShapelessRecipe
        {
            Id = Key(recipeKey)
        };
        configure(recipe);
        RegisterRecipe(recipe);
        return recipe;
    }

    /// <summary>
    /// Create and register a shaped recipe
    /// </summary>
    public ShapedRecipe RegisterShapedRecipe(string recipeKey, Action<ShapedRecipe> configure)
    {
        var recipe = new ShapedRecipe
        {
            Id = Key(recipeKey)
        };
        configure(recipe);
        RegisterRecipe(recipe);
        return recipe;
    }

    /// <summary>
    /// Create and register a smelting recipe
    /// </summary>
    public SmeltingRecipe RegisterSmeltingRecipe(string recipeKey, Action<SmeltingRecipe> configure)
    {
        var recipe = new SmeltingRecipe
        {
            Id = Key(recipeKey)
        };
        configure(recipe);
        RegisterRecipe(recipe);
        return recipe;
    }

    /// <summary>
    /// Register a custom recipe type
    /// </summary>
    public void RegisterRecipeType(string typeKey, Type recipeType)
    {
        RecipeRegistry.Instance.RegisterRecipeType(Key(typeKey), recipeType);
    }

    #endregion

    #region Dimensions

    /// <summary>
    /// Register a dimension
    /// </summary>
    public void RegisterDimension(Dimension dimension)
    {
        DimensionRegistry.Instance.Register(dimension);
    }

    /// <summary>
    /// Create and register a simple dimension
    /// </summary>
    public SimpleDimension RegisterSimpleDimension(string key, string name, Action<SimpleDimension>? configure = null)
    {
        var dimension = new SimpleDimension(Key(key), name);
        configure?.Invoke(dimension);
        RegisterDimension(dimension);
        return dimension;
    }

    #endregion

    /// <summary>
    /// Log a message for this mod
    /// </summary>
    public void Log(string message) => Logger.Info($"[{_mod.Info.ModId}] {message}");

    /// <summary>
    /// Log a warning for this mod
    /// </summary>
    public void LogWarning(string message) => Logger.Warning($"[{_mod.Info.ModId}] {message}");

    /// <summary>
    /// Log an error for this mod
    /// </summary>
    public void LogError(string message) => Logger.Error($"[{_mod.Info.ModId}] {message}");
}

/// <summary>
/// Extension methods to make Mod class easier to use
/// </summary>
public static class ModExtensions
{
    /// <summary>
    /// Get the API helper for this mod
    /// </summary>
    public static ModAPI GetAPI(this Mod mod)
    {
        return new ModAPI(mod);
    }
}
