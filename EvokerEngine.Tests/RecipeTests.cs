using Xunit;
using EvokerEngine.Crafting;
using EvokerEngine.Core;
using System.Collections.Generic;

namespace EvokerEngine.Tests;

public class RecipeTests
{
    [Fact]
    public void ShapelessRecipe_CanCraftWithSufficientIngredients()
    {
        // Arrange
        var recipe = new ShapelessRecipe
        {
            Id = new ResourceKey("test", "recipe1"),
            Name = "Test Recipe"
        };
        recipe.Ingredients.Add(new Ingredient("test:item1", 2));
        recipe.Ingredients.Add(new Ingredient("test:item2", 1));
        recipe.Result = new RecipeResult("test:result", 1);

        var available = new Dictionary<ResourceKey, int>
        {
            { "test:item1", 5 },
            { "test:item2", 3 }
        };

        // Act
        var canCraft = recipe.CanCraft(available);

        // Assert
        Assert.True(canCraft);
    }

    [Fact]
    public void ShapelessRecipe_CannotCraftWithInsufficientIngredients()
    {
        // Arrange
        var recipe = new ShapelessRecipe
        {
            Id = new ResourceKey("test", "recipe2")
        };
        recipe.Ingredients.Add(new Ingredient("test:item1", 5));
        recipe.Result = new RecipeResult("test:result", 1);

        var available = new Dictionary<ResourceKey, int>
        {
            { "test:item1", 3 }
        };

        // Act
        var canCraft = recipe.CanCraft(available);

        // Assert
        Assert.False(canCraft);
    }

    [Fact]
    public void ShapelessRecipe_ConsumesIngredientsWhenCrafting()
    {
        // Arrange
        var recipe = new ShapelessRecipe
        {
            Id = new ResourceKey("test", "recipe3")
        };
        recipe.Ingredients.Add(new Ingredient("test:wood", 4));
        recipe.Result = new RecipeResult("test:planks", 16);

        var available = new Dictionary<ResourceKey, int>
        {
            { "test:wood", 10 }
        };

        // Act
        recipe.Craft(available);

        // Assert
        Assert.Equal(6, available["test:wood"]);
    }

    [Fact]
    public void ShapedRecipe_MatchesPattern()
    {
        // Arrange
        var recipe = new ShapedRecipe
        {
            Id = new ResourceKey("test", "sword"),
            Pattern = new List<string> { " I ", " I ", " S " }
        };
        recipe.PatternKey['I'] = new Ingredient("test:iron", 1);
        recipe.PatternKey['S'] = new Ingredient("test:stick", 1);
        recipe.Result = new RecipeResult("test:iron_sword", 1);

        var available = new Dictionary<ResourceKey, int>
        {
            { "test:iron", 2 },
            { "test:stick", 1 }
        };

        // Act
        var canCraft = recipe.CanCraft(available);

        // Assert
        Assert.True(canCraft);
    }

    [Fact]
    public void SmeltingRecipe_HasCorrectType()
    {
        // Arrange & Act
        var recipe = new SmeltingRecipe
        {
            Id = new ResourceKey("test", "smelt_iron")
        };

        // Assert
        Assert.Equal("evoker:smelting", recipe.Type.FullKey);
    }

    [Fact]
    public void RecipeRegistry_RegistersRecipes()
    {
        // Arrange
        var registry = RecipeRegistry.Instance;
        var recipe = new ShapelessRecipe
        {
            Id = new ResourceKey("test", "test_recipe"),
            Type = new ResourceKey("evoker", "shapeless_crafting")
        };
        recipe.Result = new RecipeResult("test:result", 1);

        // Act
        registry.Register(recipe);

        // Assert
        Assert.True(registry.IsRegistered(recipe.Id));
    }

    [Fact]
    public void RecipeRegistry_FindsCraftableRecipes()
    {
        // Arrange
        var registry = RecipeRegistry.Instance;
        var recipe = new ShapelessRecipe
        {
            Id = new ResourceKey("test", "findable_recipe"),
            Type = new ResourceKey("evoker", "shapeless_crafting")
        };
        recipe.Ingredients.Add(new Ingredient("test:wood", 1));
        recipe.Result = new RecipeResult("test:planks", 4);
        registry.Register(recipe);

        var available = new Dictionary<ResourceKey, int>
        {
            { "test:wood", 5 }
        };

        // Act
        var craftable = registry.FindCraftableRecipes(available);

        // Assert
        Assert.Contains(recipe, craftable);
    }

    [Fact]
    public void RecipeRegistry_CanRegisterCustomRecipeType()
    {
        // Arrange
        var registry = RecipeRegistry.Instance;

        // Act
        registry.RegisterRecipeType(new ResourceKey("test", "custom"), typeof(ShapelessRecipe));

        // Assert
        Assert.True(registry.IsRecipeTypeRegistered(new ResourceKey("test", "custom")));
    }
}
