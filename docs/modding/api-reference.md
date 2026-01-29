---
tags:
  - modding
  - api
  - extensibility
---

# Modding API Reference

Complete API reference for creating mods in Evoker Engine.

## Mod Base Class

```csharp
public abstract class Mod
{
    public ModInfo Info { get; protected set; }
    public bool IsLoaded { get; }
    
    // Lifecycle methods
    public virtual void OnLoad() { }
    public virtual void OnInitialize() { }
    public virtual void OnPostInitialize() { }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnUnload() { }
    
    // Logging
    protected void Log(string message);
    protected void LogWarning(string message);
    protected void LogError(string message);
}
```

## ModInfo

```csharp
public class ModInfo
{
    public string ModId { get; set; }           // Unique identifier
    public string Name { get; set; }            // Display name
    public string Version { get; set; }         // Semantic version
    public string Author { get; set; }          // Author name
    public string Description { get; set; }     // Mod description
    public List<string> Dependencies { get; set; }  // Required mods
}
```

## ModAPI Helper

```csharp
public static class ModAPI
{
    // Blocks
    public static void RegisterBlock(Block block);
    public static Block? GetBlock(ResourceKey id);
    
    // Items
    public static void RegisterItem(Item item);
    public static Item? GetItem(ResourceKey id);
    
    // Recipes
    public static void RegisterRecipe(Recipe recipe);
    public static Recipe? GetRecipe(ResourceKey id);
    
    // Dimensions
    public static void RegisterDimension(Dimension dimension);
    public static Dimension? GetDimension(ResourceKey id);
}
```

## Quick Reference

### Creating a Basic Mod

```csharp
using EvokerEngine.Modding;
using EvokerEngine.Core;

public class MyMod : Mod
{
    public MyMod()
    {
        Info = new ModInfo
        {
            ModId = "mymod",
            Name = "My Awesome Mod",
            Version = "1.0.0",
            Author = "YourName",
            Description = "Adds cool new features"
        };
    }

    public override void OnLoad()
    {
        base.OnLoad();
        Log("Mod loaded!");
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        // Register content
        RegisterBlocks();
        RegisterItems();
        RegisterRecipes();
    }

    private void RegisterBlocks()
    {
        var customBlock = new CustomBlock();
        ModAPI.RegisterBlock(customBlock);
    }

    private void RegisterItems()
    {
        var customItem = new SimpleItem("mymod:cool_item", "Cool Item");
        ModAPI.RegisterItem(customItem);
    }

    private void RegisterRecipes()
    {
        var recipe = new ShapelessRecipe
        {
            Id = new ResourceKey("mymod", "cool_recipe"),
            Ingredients = new() { /* ... */ },
            Result = new RecipeResult("mymod:cool_item", 1)
        };
        ModAPI.RegisterRecipe(recipe);
    }
}
```

### Adding Custom Blocks

```csharp
public class CustomBlock : Block
{
    public CustomBlock()
    {
        Id = new ResourceKey("mymod", "custom_block");
        Name = "Custom Block";
        Hardness = 2.0f;
        LightLevel = 10;
    }

    public override void OnPlaced(Vector3 position, object? context = null)
    {
        Logger.Info($"Custom block placed at {position}");
    }
}
```

### Adding Custom Items

```csharp
public class CustomTool : Item
{
    public CustomTool()
    {
        Id = new ResourceKey("mymod", "custom_tool");
        Name = "Custom Tool";
        MaxStackSize = 1;
        Value = 100;
    }

    public override bool OnUse(object? context = null)
    {
        Logger.Info("Custom tool used!");
        return false;  // Not consumed
    }
}
```

### Adding Dimensions

```csharp
var customDimension = new Dimension
{
    Id = new ResourceKey("mymod", "custom_dimension"),
    Name = "Custom Dimension",
    Type = DimensionType.Custom,
    HasSky = true,
    Gravity = 0.5f
};

ModAPI.RegisterDimension(customDimension);
```

## Mod Lifecycle

1. **OnLoad()** - Mod DLL loaded
2. **OnInitialize()** - Register content (blocks, items, recipes)
3. **OnPostInitialize()** - All mods initialized
4. **OnUpdate(deltaTime)** - Called every frame
5. **OnUnload()** - Mod being unloaded

## ResourceKey Format

All IDs use the `namespace:key` format:

```csharp
// Your mod namespace
var id = new ResourceKey("mymod", "item_name");

// Built-in namespace
var vanillaId = new ResourceKey("evoker", "stone");

// Parse from string
var parsed = ResourceKey.Parse("mymod:cool_block");
```

## Dependencies

```csharp
public MyMod()
{
    Info = new ModInfo
    {
        ModId = "mymod",
        Name = "My Mod",
        Dependencies = new List<string>
        {
            "evoker",      // Base engine
            "othermod"     // Another mod
        }
    };
}
```

## API Categories

### Block Registration
- `ModAPI.RegisterBlock(block)`
- `BlockRegistry.Instance.Register(block)`

### Item Registration
- `ModAPI.RegisterItem(item)`
- Custom item classes inheriting from `Item`

### Recipe Registration
- `ModAPI.RegisterRecipe(recipe)`
- `RecipeRegistry.Instance.Register(recipe)`
- Support for Shapeless, Shaped, and Smelting recipes

### Dimension Registration
- `ModAPI.RegisterDimension(dimension)`
- `DimensionRegistry.Instance.Register(dimension)`

## See Also

- [Mod Examples](examples.md) - Complete mod examples
- [Creating Mods](creating-mods.md) - Detailed mod creation guide
