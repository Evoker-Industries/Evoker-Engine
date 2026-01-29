---
tags:
  - gamesystem
---

# Crafting System

The Crafting system in Evoker Engine provides a flexible recipe system supporting multiple recipe types including shapeless crafting, shaped crafting (grid-based), smelting, and custom recipe types.

## Overview

The crafting system includes:

- **Recipes**: Define how items are created from ingredients
- **Recipe Types**: Shapeless, Shaped, Smelting, and custom types
- **Recipe Registry**: Central management for all recipes
- **Ingredients**: Required items with quantities
- **Results**: Output items with quantities

## Recipe Types

### ShapelessRecipe

Order-independent crafting (ingredients can be in any arrangement).

```csharp
var mushroomStew = new ShapelessRecipe
{
    Id = new ResourceKey("game", "mushroom_stew"),
    Type = new ResourceKey("evoker", "shapeless_crafting"),
    Name = "Mushroom Stew",
    Ingredients = new List<Ingredient>
    {
        new Ingredient("game:red_mushroom", 1),
        new Ingredient("game:brown_mushroom", 1),
        new Ingredient("game:bowl", 1)
    },
    Result = new RecipeResult("game:mushroom_stew", 1)
};
```

### ShapedRecipe

Pattern-based crafting in a 3x3 grid.

```csharp
var pickaxe = new ShapedRecipe
{
    Id = new ResourceKey("game", "iron_pickaxe"),
    Type = new ResourceKey("evoker", "shaped_crafting"),
    Name = "Iron Pickaxe",
    Width = 3,
    Height = 3,
    Pattern = new[]
    {
        "III",  // Top row: 3 iron ingots
        " S ",  // Middle row: 1 stick (center)
        " S "   // Bottom row: 1 stick (center)
    },
    IngredientMap = new Dictionary<char, ResourceKey>
    {
        { 'I', new ResourceKey("game", "iron_ingot") },
        { 'S', new ResourceKey("game", "stick") },
        { ' ', ResourceKey.Empty }  // Empty space
    },
    Result = new RecipeResult("game:iron_pickaxe", 1)
};
```

### SmeltingRecipe

Furnace/smelting recipes.

```csharp
var ironIngot = new SmeltingRecipe
{
    Id = new ResourceKey("game", "iron_ingot_smelting"),
    Type = new ResourceKey("evoker", "smelting"),
    Name = "Iron Ingot",
    Ingredients = new List<Ingredient>
    {
        new Ingredient("game:iron_ore", 1)
    },
    Result = new RecipeResult("game:iron_ingot", 1),
    CookTime = 10f,  // seconds
    Experience = 0.7f
};
```

## Recipe Components

### Ingredient

```csharp
public class Ingredient
{
    public ResourceKey ItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public Dictionary<string, object>? RequiredProperties { get; set; }
}

// Simple ingredient
var ingredient = new Ingredient("game:wood", 1);

// With required properties
var enchantedBook = new Ingredient("game:enchanted_book", 1)
{
    RequiredProperties = new Dictionary<string, object>
    {
        { "enchantment", "sharpness" },
        { "level", 5 }
    }
};
```

### RecipeResult

```csharp
public class RecipeResult
{
    public ResourceKey ItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public Dictionary<string, object>? Properties { get; set; }
}

// Simple result
var result = new RecipeResult("game:sword", 1);

// With properties
var enchantedSword = new RecipeResult("game:diamond_sword", 1)
{
    Properties = new Dictionary<string, object>
    {
        { "enchantment", "sharpness" },
        { "durability", 100 }
    }
};
```

## Recipe Registry

```csharp
public class RecipeRegistry
{
    public static RecipeRegistry Instance { get; }
    
    public void RegisterRecipeType(ResourceKey typeId, Type recipeType);
    public void Register(Recipe recipe);
    public void RegisterAll(params Recipe[] recipes);
    public Recipe? Get(ResourceKey id);
    public Recipe? Get(string id);
    public IEnumerable<Recipe> GetRecipesByType(ResourceKey typeId);
    public IEnumerable<Recipe> GetAll();
    public Recipe? FindMatchingRecipe(List<Ingredient> ingredients, ResourceKey? recipeType = null);
}
```

### Registering Recipes

```csharp
using EvokerEngine.Crafting;

// Register single recipe
var recipe = new ShapelessRecipe { /* ... */ };
RecipeRegistry.Instance.Register(recipe);

// Register multiple recipes
RecipeRegistry.Instance.RegisterAll(
    mushroomStew,
    ironPickaxe,
    ironIngotSmelting
);

// Get recipe
var recipe = RecipeRegistry.Instance.Get("game:iron_pickaxe");
```

## Custom Recipe Types

Create your own recipe types:

```csharp
public class BrewingRecipe : Recipe
{
    public ResourceKey PotionBase { get; set; }  // Base potion
    public ResourceKey BrewingIngredient { get; set; }  // Added ingredient
    public float BrewTime { get; set; } = 20f;

    public override bool Matches(List<Ingredient> ingredients)
    {
        // Custom matching logic
        return ingredients.Any(i => i.ItemId == PotionBase) &&
               ingredients.Any(i => i.ItemId == BrewingIngredient);
    }
}

// Register the custom recipe type
RecipeRegistry.Instance.RegisterRecipeType(
    new ResourceKey("game", "brewing"),
    typeof(BrewingRecipe)
);

// Create brewing recipe
var strengthPotion = new BrewingRecipe
{
    Id = new ResourceKey("game", "strength_potion"),
    Type = new ResourceKey("game", "brewing"),
    PotionBase = new ResourceKey("game", "awkward_potion"),
    BrewingIngredient = new ResourceKey("game", "blaze_powder"),
    Result = new RecipeResult("game:strength_potion", 1),
    BrewTime = 20f
};
```

## Complete Examples

### Example 1: Crafting Table System

```csharp
public class CraftingSystem
{
    private List<Ingredient> _craftingGrid = new();

    public bool TryCraft()
    {
        var recipe = RecipeRegistry.Instance.FindMatchingRecipe(
            _craftingGrid,
            new ResourceKey("evoker", "shaped_crafting")
        );

        if (recipe != null)
        {
            // Remove ingredients
            RemoveIngredients(recipe.Ingredients);
            
            // Give result
            GiveItem(recipe.Result.ItemId, recipe.Result.Quantity);
            
            Logger.Info($"Crafted: {recipe.Name}");
            return true;
        }
        
        return false;
    }

    private void RemoveIngredients(List<Ingredient> ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            // Remove from inventory
        }
    }

    private void GiveItem(ResourceKey itemId, int quantity)
    {
        // Add to inventory
    }
}
```

### Example 2: Complete Recipe Set

```csharp
public static class GameRecipes
{
    public static void RegisterAll()
    {
        // Tools
        RegisterToolRecipes();
        
        // Weapons
        RegisterWeaponRecipes();
        
        // Food
        RegisterFoodRecipes();
        
        // Smelting
        RegisterSmeltingRecipes();
    }

    private static void RegisterToolRecipes()
    {
        // Wooden Pickaxe
        RecipeRegistry.Instance.Register(new ShapedRecipe
        {
            Id = new ResourceKey("game", "wooden_pickaxe"),
            Type = new ResourceKey("evoker", "shaped_crafting"),
            Pattern = new[] { "PPP", " S ", " S " },
            IngredientMap = new Dictionary<char, ResourceKey>
            {
                { 'P', new ResourceKey("game", "planks") },
                { 'S', new ResourceKey("game", "stick") }
            },
            Result = new RecipeResult("game:wooden_pickaxe", 1)
        });

        // Add more tools...
    }

    private static void RegisterFoodRecipes()
    {
        // Bread
        RecipeRegistry.Instance.Register(new ShapelessRecipe
        {
            Id = new ResourceKey("game", "bread"),
            Type = new ResourceKey("evoker", "shapeless_crafting"),
            Ingredients = new List<Ingredient>
            {
                new Ingredient("game:wheat", 3)
            },
            Result = new RecipeResult("game:bread", 1)
        });
    }

    private static void RegisterSmeltingRecipes()
    {
        // Iron Ingot
        RecipeRegistry.Instance.Register(new SmeltingRecipe
        {
            Id = new ResourceKey("game", "iron_ingot"),
            Type = new ResourceKey("evoker", "smelting"),
            Ingredients = new List<Ingredient>
            {
                new Ingredient("game:iron_ore", 1)
            },
            Result = new RecipeResult("game:iron_ingot", 1),
            CookTime = 10f,
            Experience = 0.7f
        });

        // Gold Ingot
        RecipeRegistry.Instance.Register(new SmeltingRecipe
        {
            Id = new ResourceKey("game", "gold_ingot"),
            Type = new ResourceKey("evoker", "smelting"),
            Ingredients = new List<Ingredient>
            {
                new Ingredient("game:gold_ore", 1)
            },
            Result = new RecipeResult("game:gold_ingot", 1),
            CookTime = 10f,
            Experience = 1.0f
        });
    }
}
```

## Best Practices

### ✅ Do's

- Use ResourceKey format for all IDs
- Group related recipes together
- Set appropriate cook times for smelting
- Use ShapelessRecipe when order doesn't matter
- Register all recipes at startup

### ❌ Don'ts

- Don't create recipes with impossible ingredient requirements
- Don't forget to register custom recipe types first
- Don't duplicate recipe IDs
- Don't make recipes that create more resources than inputs without balance

## See Also

- [Inventory](inventory.md) - Item system
- [Blocks](blocks.md) - Block system for crafting blocks as ingredients
