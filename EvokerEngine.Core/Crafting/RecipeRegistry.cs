using System;
using System.Collections.Generic;
using System.Linq;
using EvokerEngine.Core;

namespace EvokerEngine.Crafting;

/// <summary>
/// Registry for managing recipes and recipe types
/// </summary>
public class RecipeRegistry
{
    private static RecipeRegistry? _instance;
    private readonly Dictionary<ResourceKey, Recipe> _recipes = new();
    private readonly Dictionary<ResourceKey, Type> _recipeTypes = new();

    /// <summary>
    /// Singleton instance of the recipe registry
    /// </summary>
    public static RecipeRegistry Instance => _instance ??= new RecipeRegistry();

    private RecipeRegistry()
    {
        // Register built-in recipe types
        RegisterRecipeType(new ResourceKey("evoker", "shapeless_crafting"), typeof(ShapelessRecipe));
        RegisterRecipeType(new ResourceKey("evoker", "shaped_crafting"), typeof(ShapedRecipe));
        RegisterRecipeType(new ResourceKey("evoker", "smelting"), typeof(SmeltingRecipe));
    }

    /// <summary>
    /// Register a custom recipe type
    /// </summary>
    public void RegisterRecipeType(ResourceKey typeId, Type recipeType)
    {
        if (!typeof(Recipe).IsAssignableFrom(recipeType))
            throw new ArgumentException($"Type {recipeType.Name} must inherit from Recipe");

        if (_recipeTypes.ContainsKey(typeId))
            throw new InvalidOperationException($"Recipe type '{typeId}' is already registered");

        _recipeTypes[typeId] = recipeType;
        Logger.Info($"Registered recipe type: {typeId} ({recipeType.Name})");
    }

    /// <summary>
    /// Register a recipe
    /// </summary>
    public void Register(Recipe recipe)
    {
        if (recipe == null)
            throw new ArgumentNullException(nameof(recipe));

        if (_recipes.ContainsKey(recipe.Id))
            throw new InvalidOperationException($"Recipe with ID '{recipe.Id}' is already registered");

        if (!_recipeTypes.ContainsKey(recipe.Type))
            throw new InvalidOperationException($"Recipe type '{recipe.Type}' is not registered. Register the recipe type first.");

        _recipes[recipe.Id] = recipe;
        Logger.Info($"Registered recipe: {recipe.Id} (Type: {recipe.Type})");
    }

    /// <summary>
    /// Register multiple recipes
    /// </summary>
    public void RegisterAll(params Recipe[] recipes)
    {
        foreach (var recipe in recipes)
        {
            Register(recipe);
        }
    }

    /// <summary>
    /// Get a recipe by its ID
    /// </summary>
    public Recipe? Get(ResourceKey id)
    {
        return _recipes.TryGetValue(id, out var recipe) ? recipe : null;
    }

    /// <summary>
    /// Get a recipe by string ID
    /// </summary>
    public Recipe? Get(string id)
    {
        if (ResourceKey.TryParse(id, out var key))
        {
            return Get(key);
        }
        return null;
    }

    /// <summary>
    /// Get all recipes of a specific type
    /// </summary>
    public IEnumerable<Recipe> GetRecipesByType(ResourceKey typeId)
    {
        return _recipes.Values.Where(r => r.Type == typeId);
    }

    /// <summary>
    /// Get all recipes that produce a specific item
    /// </summary>
    public IEnumerable<Recipe> GetRecipesForItem(ResourceKey itemId)
    {
        return _recipes.Values.Where(r => r.Result.ItemId == itemId);
    }

    /// <summary>
    /// Find recipes that can be crafted with available items
    /// </summary>
    public IEnumerable<Recipe> FindCraftableRecipes(Dictionary<ResourceKey, int> availableItems)
    {
        return _recipes.Values.Where(r => r.CanCraft(availableItems));
    }

    /// <summary>
    /// Find recipes of a specific type that can be crafted
    /// </summary>
    public IEnumerable<Recipe> FindCraftableRecipes(ResourceKey typeId, Dictionary<ResourceKey, int> availableItems)
    {
        return GetRecipesByType(typeId).Where(r => r.CanCraft(availableItems));
    }

    /// <summary>
    /// Check if a recipe is registered
    /// </summary>
    public bool IsRegistered(ResourceKey id)
    {
        return _recipes.ContainsKey(id);
    }

    /// <summary>
    /// Check if a recipe type is registered
    /// </summary>
    public bool IsRecipeTypeRegistered(ResourceKey typeId)
    {
        return _recipeTypes.ContainsKey(typeId);
    }

    /// <summary>
    /// Get all registered recipes
    /// </summary>
    public IReadOnlyCollection<Recipe> GetAllRecipes()
    {
        return _recipes.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get all registered recipe types
    /// </summary>
    public IReadOnlyCollection<ResourceKey> GetAllRecipeTypes()
    {
        return _recipeTypes.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Unregister a recipe
    /// </summary>
    public bool Unregister(ResourceKey id)
    {
        return _recipes.Remove(id);
    }

    /// <summary>
    /// Clear all recipes (does not clear recipe types)
    /// </summary>
    public void ClearRecipes()
    {
        _recipes.Clear();
        Logger.Info("Cleared all registered recipes");
    }

    /// <summary>
    /// Clear everything including recipe types
    /// </summary>
    public void ClearAll()
    {
        _recipes.Clear();
        _recipeTypes.Clear();
        Logger.Info("Cleared all recipes and recipe types");
    }

    /// <summary>
    /// Get count of registered recipes
    /// </summary>
    public int RecipeCount => _recipes.Count;

    /// <summary>
    /// Get count of registered recipe types
    /// </summary>
    public int RecipeTypeCount => _recipeTypes.Count;
}
