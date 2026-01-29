---
tags:
  - modding
  - tutorial
  - extensibility
---

# Creating Mods for Evoker-Engine

This guide shows you how to create mods for Evoker-Engine using the modding API.

## Quick Start

### 1. Create a New Project

```bash
dotnet new classlib -n MyAwesomeMod -f net9.0
cd MyAwesomeMod
dotnet add reference path/to/EvokerEngine.Core.dll
```

### 2. Create Your Mod Class

```csharp
using EvokerEngine.Modding;
using EvokerEngine.Blocks;
using EvokerEngine.Inventory;
using EvokerEngine.Crafting;

public class MyAwesomeMod : Mod
{
    public MyAwesomeMod()
    {
        Info = new ModInfo
        {
            ModId = "mymod",  // Your mod's namespace
            Name = "My Awesome Mod",
            Version = "1.0.0",
            Author = "Your Name",
            Description = "An awesome mod for Evoker-Engine!",
            Dependencies = new List<string>()  // Other mods you depend on
        };
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        var api = this.GetAPI();  // Get the helper API
        
        // Register custom blocks
        RegisterBlocks(api);
        
        // Register custom items
        RegisterItems(api);
        
        // Register recipes
        RegisterRecipes(api);
        
        // Register dimensions
        RegisterDimensions(api);
    }

    private void RegisterBlocks(ModAPI api)
    {
        // Create a simple stone block
        api.RegisterSimpleBlock("custom_stone", "Custom Stone", block =>
        {
            block.Hardness = 2.0f;
            block.Resistance = 6.0f;
            block.IsSolid = true;
            block.IsTransparent = false;
        });

        // Create a glowing block
        api.RegisterSimpleBlock("glowstone_custom", "Custom Glowstone", block =>
        {
            block.Hardness = 0.3f;
            block.LightLevel = 15;
            block.IsSolid = true;
        });

        // Create a custom block with behavior
        var customBlock = new MyCustomBlock(api.Key("special_block"), "Special Block");
        api.RegisterBlock(customBlock);
    }

    private void RegisterItems(ModAPI api)
    {
        // Create custom items (these can be used in recipes)
        var gemItem = api.CreateSimpleItem("magic_gem", "Magic Gem");
        gemItem.MaxStackSize = 64;
        gemItem.Rarity = ItemRarity.Rare;
        gemItem.Value = 100;
    }

    private void RegisterRecipes(ModAPI api)
    {
        // Shapeless recipe example
        api.RegisterShapelessRecipe("custom_stone_from_gems", recipe =>
        {
            recipe.Name = "Custom Stone from Gems";
            recipe.Ingredients.Add(new Ingredient(api.Key("magic_gem"), 4));
            recipe.Result = new RecipeResult(api.Key("custom_stone"), 1);
        });

        // Shaped recipe example (3x3 grid pattern)
        api.RegisterShapedRecipe("special_block_recipe", recipe =>
        {
            recipe.Name = "Special Block";
            recipe.Pattern = new List<string>
            {
                "GGG",
                "GSG",
                "GGG"
            };
            recipe.PatternKey = new Dictionary<char, Ingredient>
            {
                { 'G', new Ingredient(api.Key("magic_gem"), 1) },
                { 'S', new Ingredient(api.Key("custom_stone"), 1) }
            };
            recipe.Result = new RecipeResult(api.Key("special_block"), 1);
        });

        // Smelting recipe example
        api.RegisterSmeltingRecipe("smelt_custom_stone", recipe =>
        {
            recipe.Name = "Smelt Custom Stone";
            recipe.Ingredients.Add(new Ingredient(api.Key("custom_stone"), 1));
            recipe.Result = new RecipeResult(api.Key("glowstone_custom"), 1);
            recipe.SmeltTime = 15f;  // 15 seconds
            recipe.Experience = 0.5f;
        });
    }

    private void RegisterDimensions(ModAPI api)
    {
        // Create a custom dimension
        api.RegisterSimpleDimension("crystal_realm", "Crystal Realm", dimension =>
        {
            dimension.Type = DimensionType.Custom;
            dimension.HasSky = true;
            dimension.HasCeiling = false;
            dimension.MinHeight = 0;
            dimension.MaxHeight = 256;
            dimension.CoordinateScale = 1.0f;
            dimension.AmbientLight = 12;
            dimension.FogColor = new System.Numerics.Vector3(0.8f, 0.6f, 1.0f);
            dimension.SkyColor = new System.Numerics.Vector3(0.6f, 0.4f, 0.9f);
            dimension.Gravity = 0.8f;  // Lower gravity
        });
    }

    public override void OnUpdate(float deltaTime)
    {
        // Called every frame if you need to update something
    }
}

// Custom block with special behavior
public class MyCustomBlock : Block
{
    public MyCustomBlock(ResourceKey id, string name)
    {
        Id = id;
        Name = name;
        Hardness = 3.0f;
        LightLevel = 10;
    }

    public override void OnPlaced(Vector3 position, object? context = null)
    {
        base.OnPlaced(position, context);
        Logger.Info($"Special block placed at {position}!");
    }

    public override bool OnInteract(Vector3 position, object? context = null)
    {
        Logger.Info("You interacted with the special block!");
        return true;  // Interaction handled
    }

    public override void OnUpdate(Vector3 position, float deltaTime)
    {
        // This block does something every frame
    }
}
```

### 3. Build Your Mod

```bash
dotnet build -c Release
```

### 4. Install Your Mod

Copy the built DLL to the game's `mods` folder:

```bash
cp bin/Release/net9.0/MyAwesomeMod.dll path/to/game/mods/
```

### 5. Load Your Mod

The mod will be automatically loaded when the game starts:

```csharp
// In your game's initialization code
ModLoader.Instance.LoadModsFromDirectory("mods");
ModLoader.Instance.InitializeAll();

// In your game's update loop
ModLoader.Instance.UpdateAll(deltaTime);
```

## Advanced Features

### Custom Recipe Types

You can create entirely custom recipe types:

```csharp
// Define a custom recipe type
public class AlchemyRecipe : Recipe
{
    public float BrewTime { get; set; } = 30f;
    public int ManaRequired { get; set; } = 50;

    public AlchemyRecipe()
    {
        Type = new ResourceKey("mymod", "alchemy");
    }

    public override bool CanCraft(Dictionary<ResourceKey, int> availableItems)
    {
        // Custom logic for checking if recipe can be crafted
        return base.CanCraft(availableItems);
    }

    public override RecipeResult Craft(Dictionary<ResourceKey, int> availableItems)
    {
        // Custom crafting logic
        return Result;
    }
}

// Register the custom recipe type in your mod
public override void OnInitialize()
{
    var api = this.GetAPI();
    api.RegisterRecipeType("alchemy", typeof(AlchemyRecipe));
    
    // Now you can register alchemy recipes
    var alchemyRecipe = new AlchemyRecipe
    {
        Id = api.Key("health_potion"),
        Name = "Health Potion",
        BrewTime = 45f,
        ManaRequired = 75
    };
    alchemyRecipe.Ingredients.Add(new Ingredient(api.Key("magic_gem"), 2));
    alchemyRecipe.Result = new RecipeResult(api.Key("health_potion_item"), 1);
    api.RegisterRecipe(alchemyRecipe);
}
```

### Mod Dependencies

If your mod depends on another mod:

```csharp
public MyAwesomeMod()
{
    Info = new ModInfo
    {
        ModId = "mymod",
        Name = "My Awesome Mod",
        Version = "1.0.0",
        Dependencies = new List<string> { "basemod", "utilitymod" }
    };
}
```

The mod loader will ensure dependencies are loaded first.

### Using ResourceKeys

Everything in the engine uses the `namespace:key` pattern:

```csharp
// Your mod's namespace
var myBlock = new ResourceKey("mymod", "custom_block");

// Accessing other mod's content
var otherModBlock = new ResourceKey("otherm

od", "their_block");

// Implicit string conversion
ResourceKey key = "mymod:custom_item";  // Parses automatically
string keyStr = key;  // Converts to "mymod:custom_item"

// Checking namespace
if (key.IsInNamespace("mymod"))
{
    // This is from your mod
}
```

### Custom Dimension with Special Properties

```csharp
public class MyCustomDimension : Dimension
{
    public MyCustomDimension()
    {
        Id = new ResourceKey("mymod", "void_dimension");
        Name = "The Void";
        Type = DimensionType.Custom;
        
        // Extreme properties
        HasSky = false;
        HasCeiling = false;
        MinHeight = -128;
        MaxHeight = 128;
        AmbientLight = 0;
        Gravity = 0.3f;  // Low gravity
        
        // Custom properties
        Properties["voidEnergy"] = 100.0f;
        Properties["dangerLevel"] = 10;
    }

    public override void OnUpdate(float deltaTime)
    {
        // Update void energy or other dimension-specific logic
        var energy = (float)Properties["voidEnergy"];
        energy += deltaTime * 0.1f;
        Properties["voidEnergy"] = energy;
    }
}
```

## Best Practices

1. **Use your mod's namespace**: Always use `api.Key("item_name")` for your items
2. **Log appropriately**: Use `api.Log()`, `api.LogWarning()`, `api.LogError()`
3. **Handle errors**: Wrap risky code in try-catch blocks
4. **Clean up**: Use `OnUnload()` to clean up resources
5. **Test dependencies**: Make sure all required mods are listed
6. **Version properly**: Use semantic versioning (major.minor.patch)

## Example: Complete Mini-Mod

See the full example above for a complete, working mod that adds:
- Custom blocks (stone, glowstone, special block with behavior)
- Custom items (magic gem)
- Multiple recipe types (shapeless, shaped, smelting)
- Custom dimension (crystal realm)

## API Reference

### ModAPI Methods

- `Key(string)` - Create a ResourceKey with your mod's namespace
- `RegisterBlock(Block)` - Register a block
- `RegisterSimpleBlock(key, name, configure)` - Quick block registration
- `CreateSimpleItem(key, name)` - Create a simple item
- `RegisterRecipe(Recipe)` - Register a recipe
- `RegisterShapelessRecipe(key, configure)` - Quick shapeless recipe
- `RegisterShapedRecipe(key, configure)` - Quick shaped recipe
- `RegisterSmeltingRecipe(key, configure)` - Quick smelting recipe
- `RegisterRecipeType(key, type)` - Register custom recipe type
- `RegisterDimension(Dimension)` - Register a dimension
- `RegisterSimpleDimension(key, name, configure)` - Quick dimension registration
- `Log(message)` - Log message
- `LogWarning(message)` - Log warning
- `LogError(message)` - Log error

### Mod Lifecycle

1. `OnLoad()` - Mod is being loaded
2. `OnInitialize()` - Register all content here
3. `OnPostInitialize()` - All mods initialized, safe to access other mods' content
4. `OnUpdate(deltaTime)` - Called every frame (optional)
5. `OnUnload()` - Mod is being unloaded

## Getting Help

For more examples and documentation, check the engine's core namespaces:
- `EvokerEngine.Blocks` - Block system
- `EvokerEngine.Inventory` - Items and inventory
- `EvokerEngine.Crafting` - Recipes and crafting
- `EvokerEngine.World` - Dimensions and world
- `EvokerEngine.Modding` - Mod API

Happy modding! 🎮
