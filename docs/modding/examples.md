---
tags:
  - modding
  - tutorial
  - examples
---

# Mod Examples

Complete examples of mods for Evoker Engine, from simple content mods to complex gameplay additions.

## Example 1: Simple Item Mod

Add new items to the game.

```csharp
using EvokerEngine.Modding;
using EvokerEngine.Inventory;
using EvokerEngine.Core;

public class SimpleItemMod : Mod
{
    public SimpleItemMod()
    {
        Info = new ModInfo
        {
            ModId = "simpleitems",
            Name = "Simple Items Mod",
            Version = "1.0.0",
            Author = "ModAuthor",
            Description = "Adds basic items to the game"
        };
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        // Ruby gem
        var ruby = new SimpleItem("simpleitems:ruby", "Ruby")
        {
            Description = "A precious red gemstone",
            MaxStackSize = 64,
            Value = 500,
            Weight = 0.1f,
            Rarity = ItemRarity.Rare
        };
        ModAPI.RegisterItem(ruby);
        
        // Emerald gem
        var emerald = new SimpleItem("simpleitems:emerald", "Emerald")
        {
            Description = "A valuable green gemstone",
            MaxStackSize = 64,
            Value = 750,
            Weight = 0.1f,
            Rarity = ItemRarity.Epic
        };
        ModAPI.RegisterItem(emerald);
        
        // Magic potion
        var magicPotion = new MagicPotion();
        ModAPI.RegisterItem(magicPotion);
        
        Log("Registered 3 new items");
    }
}

public class MagicPotion : Item
{
    public MagicPotion()
    {
        Id = new ResourceKey("simpleitems", "magic_potion");
        Name = "Magic Potion";
        Description = "Restores mana";
        MaxStackSize = 10;
        Value = 100;
        Rarity = ItemRarity.Uncommon;
    }

    public override bool OnUse(object? context = null)
    {
        // Restore mana logic
        Logger.Info("Mana restored!");
        return true;  // Consumed
    }
}
```

## Example 2: Block Mod with Ores

Add new ore blocks and materials.

```csharp
using EvokerEngine.Modding;
using EvokerEngine.Blocks;
using EvokerEngine.Inventory;
using EvokerEngine.Core;

public class OresMod : Mod
{
    public OresMod()
    {
        Info = new ModInfo
        {
            ModId = "oresmod",
            Name = "Ores Expansion",
            Version = "1.0.0",
            Author = "MinerCrafter",
            Description = "Adds new ores and materials"
        };
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        // Register ore items
        RegisterOreItems();
        
        // Register ore blocks
        RegisterOreBlocks();
        
        // Register smelting recipes
        RegisterRecipes();
    }

    private void RegisterOreItems()
    {
        var copperIngot = new SimpleItem("oresmod:copper_ingot", "Copper Ingot")
        {
            Description = "Refined copper",
            MaxStackSize = 64,
            Value = 50
        };
        ModAPI.RegisterItem(copperIngot);
        
        var tinIngot = new SimpleItem("oresmod:tin_ingot", "Tin Ingot")
        {
            Description = "Refined tin",
            MaxStackSize = 64,
            Value = 40
        };
        ModAPI.RegisterItem(tinIngot);
        
        var bronzeIngot = new SimpleItem("oresmod:bronze_ingot", "Bronze Ingot")
        {
            Description = "Copper and tin alloy",
            MaxStackSize = 64,
            Value = 100
        };
        ModAPI.RegisterItem(bronzeIngot);
    }

    private void RegisterOreBlocks()
    {
        var copperOre = new OreBlock("Copper Ore", new ResourceKey("oresmod", "copper_ingot"))
        {
            Hardness = 3.0f,
            MinDrops = 1,
            MaxDrops = 1
        };
        ModAPI.RegisterBlock(copperOre);
        
        var tinOre = new OreBlock("Tin Ore", new ResourceKey("oresmod", "tin_ingot"))
        {
            Hardness = 2.5f,
            MinDrops = 1,
            MaxDrops = 1
        };
        ModAPI.RegisterBlock(tinOre);
    }

    private void RegisterRecipes()
    {
        // Smelt copper ore
        var copperSmelting = new SmeltingRecipe
        {
            Id = new ResourceKey("oresmod", "copper_smelting"),
            Type = new ResourceKey("evoker", "smelting"),
            Ingredients = new() { new Ingredient("oresmod:copper_ore", 1) },
            Result = new RecipeResult("oresmod:copper_ingot", 1),
            CookTime = 10f
        };
        ModAPI.RegisterRecipe(copperSmelting);
        
        // Bronze alloy (crafting)
        var bronzeRecipe = new ShapelessRecipe
        {
            Id = new ResourceKey("oresmod", "bronze_crafting"),
            Type = new ResourceKey("evoker", "shapeless_crafting"),
            Ingredients = new()
            {
                new Ingredient("oresmod:copper_ingot", 3),
                new Ingredient("oresmod:tin_ingot", 1)
            },
            Result = new RecipeResult("oresmod:bronze_ingot", 4)
        };
        ModAPI.RegisterRecipe(bronzeRecipe);
        
        Log("Registered smelting and crafting recipes");
    }
}

public class OreBlock : Block
{
    public ResourceKey DropItemId { get; set; }
    public int MinDrops { get; set; } = 1;
    public int MaxDrops { get; set; } = 1;

    public OreBlock(string name, ResourceKey dropItem)
    {
        Id = new ResourceKey("oresmod", name.ToLower().Replace(" ", "_"));
        Name = name;
        Hardness = 3.0f;
        DropItemId = dropItem;
    }

    public override List<(ResourceKey itemId, int quantity)> GetDrops(object? context = null)
    {
        var amount = Random.Shared.Next(MinDrops, MaxDrops + 1);
        return new List<(ResourceKey, int)> { (DropItemId, amount) };
    }
}
```

## Example 3: Custom Dimension Mod

Add a new dimension with unique properties.

```csharp
using EvokerEngine.Modding;
using EvokerEngine.World;
using EvokerEngine.Core;
using System.Numerics;

public class SkyworldMod : Mod
{
    public SkyworldMod()
    {
        Info = new ModInfo
        {
            ModId = "skyworld",
            Name = "Skyworld Dimension",
            Version = "1.0.0",
            Author = "CloudBuilder",
            Description = "Adds a floating island dimension"
        };
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        var skyworld = new Dimension
        {
            Id = new ResourceKey("skyworld", "skyworld"),
            Name = "Skyworld",
            Type = DimensionType.Custom,
            
            // Bright and open
            AmbientLight = 15,
            HasSky = true,
            HasCeiling = false,
            SkyColor = new Vector3(0.4f, 0.7f, 1.0f),
            FogColor = new Vector3(0.9f, 0.95f, 1.0f),
            
            // High altitude
            MinHeight = 0,
            MaxHeight = 512,
            
            // Lighter gravity
            Gravity = 0.7f,
            
            // Normal gameplay
            BedsWork = true,
            WaterEvaporates = false
        };
        
        ModAPI.RegisterDimension(skyworld);
        
        // Create portal block
        var portal = new SkyworldPortal();
        ModAPI.RegisterBlock(portal);
        
        Log("Skyworld dimension registered");
    }
}

public class SkyworldPortal : Block
{
    public SkyworldPortal()
    {
        Id = new ResourceKey("skyworld", "skyworld_portal");
        Name = "Skyworld Portal";
        IsTransparent = true;
        LightLevel = 15;
    }

    public override bool OnInteract(Vector3 position, object? context = null)
    {
        // Teleport player to Skyworld
        Logger.Info("Teleporting to Skyworld!");
        // Teleportation logic here
        return true;
    }
}
```

## Example 4: Tool and Weapon Mod

Add custom tools and weapons with durability.

```csharp
using EvokerEngine.Modding;
using EvokerEngine.Inventory;
using EvokerEngine.Core;

public class ToolsMod : Mod
{
    public ToolsMod()
    {
        Info = new ModInfo
        {
            ModId = "customtools",
            Name = "Custom Tools",
            Version = "1.0.0",
            Author = "Blacksmith",
            Description = "Adds new tools and weapons"
        };
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        // Ruby tools
        var rubySword = new RubySword();
        var rubyPickaxe = new RubyPickaxe();
        
        ModAPI.RegisterItem(rubySword);
        ModAPI.RegisterItem(rubyPickaxe);
        
        // Tool recipes
        RegisterToolRecipes();
    }

    private void RegisterToolRecipes()
    {
        // Ruby Sword
        var swordRecipe = new ShapedRecipe
        {
            Id = new ResourceKey("customtools", "ruby_sword"),
            Type = new ResourceKey("evoker", "shaped_crafting"),
            Pattern = new[] { " R ", " R ", " S " },
            IngredientMap = new Dictionary<char, ResourceKey>
            {
                { 'R', new ResourceKey("simpleitems", "ruby") },
                { 'S', new ResourceKey("evoker", "stick") }
            },
            Result = new RecipeResult("customtools:ruby_sword", 1)
        };
        ModAPI.RegisterRecipe(swordRecipe);
        
        // Ruby Pickaxe
        var pickaxeRecipe = new ShapedRecipe
        {
            Id = new ResourceKey("customtools", "ruby_pickaxe"),
            Type = new ResourceKey("evoker", "shaped_crafting"),
            Pattern = new[] { "RRR", " S ", " S " },
            IngredientMap = new Dictionary<char, ResourceKey>
            {
                { 'R', new ResourceKey("simpleitems", "ruby") },
                { 'S', new ResourceKey("evoker", "stick") }
            },
            Result = new RecipeResult("customtools:ruby_pickaxe", 1)
        };
        ModAPI.RegisterRecipe(pickaxeRecipe);
    }
}

public class RubySword : Item
{
    private int _durability = 500;
    private int _maxDurability = 500;

    public RubySword()
    {
        Id = new ResourceKey("customtools", "ruby_sword");
        Name = "Ruby Sword";
        Description = "A powerful sword made of ruby";
        MaxStackSize = 1;
        Value = 1000;
        Rarity = ItemRarity.Rare;
    }

    public override bool OnUse(object? context = null)
    {
        // Attack logic
        _durability--;
        
        if (_durability <= 0)
        {
            Logger.Info("Ruby Sword broke!");
            return true;  // Item consumed (broken)
        }
        
        Logger.Info($"Ruby Sword durability: {_durability}/{_maxDurability}");
        return false;  // Not consumed
    }
}

public class RubyPickaxe : Item
{
    private int _durability = 800;

    public RubyPickaxe()
    {
        Id = new ResourceKey("customtools", "ruby_pickaxe");
        Name = "Ruby Pickaxe";
        Description = "An efficient pickaxe for mining";
        MaxStackSize = 1;
        Value = 1200;
        Rarity = ItemRarity.Rare;
    }
}
```

## Example 5: Gameplay Mechanics Mod

Add new gameplay mechanics with update logic.

```csharp
using EvokerEngine.Modding;
using EvokerEngine.Core;

public class SeasonsMod : Mod
{
    private float _seasonTime = 0f;
    private const float SEASON_DURATION = 600f;  // 10 minutes
    private Season _currentSeason = Season.Spring;

    public SeasonsMod()
    {
        Info = new ModInfo
        {
            ModId = "seasons",
            Name = "Seasons",
            Version = "1.0.0",
            Author = "WeatherMan",
            Description = "Adds seasonal changes to the game"
        };
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        Log("Seasons system initialized - Starting in Spring");
    }

    public override void OnUpdate(float deltaTime)
    {
        _seasonTime += deltaTime;
        
        if (_seasonTime >= SEASON_DURATION)
        {
            _seasonTime = 0f;
            ChangeSeason();
        }
    }

    private void ChangeSeason()
    {
        _currentSeason = _currentSeason switch
        {
            Season.Spring => Season.Summer,
            Season.Summer => Season.Autumn,
            Season.Autumn => Season.Winter,
            Season.Winter => Season.Spring,
            _ => Season.Spring
        };
        
        Log($"Season changed to: {_currentSeason}");
        ApplySeasonEffects();
    }

    private void ApplySeasonEffects()
    {
        switch (_currentSeason)
        {
            case Season.Spring:
                // Increase crop growth
                Log("Crops grow faster in Spring");
                break;
            case Season.Summer:
                // Increase temperature
                Log("Hot summer days");
                break;
            case Season.Autumn:
                // Change foliage colors
                Log("Leaves change color");
                break;
            case Season.Winter:
                // Add snow
                Log("Winter snowfall begins");
                break;
        }
    }

    private enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
}
```

## Best Practices

### ✅ Do's

- Use unique mod IDs (namespace)
- Document your mod's features
- Handle dependencies properly
- Test with and without other mods
- Provide configuration options
- Use semantic versioning

### ❌ Don'ts

- Don't use generic mod IDs
- Don't hardcode values (use config files)
- Don't break compatibility without version bump
- Don't access private engine internals
- Don't forget to clean up resources in OnUnload()

## See Also

- [API Reference](api-reference.md) - Complete API documentation
- [Creating Mods](creating-mods.md) - Mod creation guide
