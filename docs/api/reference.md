# API Reference Overview

This page provides a comprehensive overview of the Evoker Engine API with links to detailed documentation.

## Core Systems

### [Application](../core/application.md)
Main application class managing the engine lifecycle.

```csharp
var app = new Application("My Game", 1280, 720);
app.PushLayer(new GameLayer());
app.Run();
```

**Key Classes:**
- `Application` - Main application singleton
- `Time` - Delta time, FPS, time scaling
- `Logger` - Logging system
- `Platform` - Cross-platform utilities
- `ResourceKey` - namespace:key identifier system

### [Layer System](../core/layers.md)
Modular layer-based architecture for organizing game logic.

```csharp
public class GameLayer : Layer
{
    public override void OnAttach() { }
    public override void OnUpdate(float deltaTime) { }
    public override void OnRender() { }
    public override void OnEvent(Event e) { }
    public override void OnDetach() { }
}
```

**Key Classes:**
- `Layer` - Base layer class
- `LayerStack` - Layer management

### [Event System](../core/events.md)
Event handling for input and window events.

```csharp
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent)
    {
        // Handle key press
    }
}
```

**Event Types:**
- Window events: `WindowCloseEvent`, `WindowResizeEvent`
- Keyboard events: `KeyPressedEvent`, `KeyReleasedEvent`
- Mouse events: `MouseButtonPressedEvent`, `MouseMovedEvent`, `MouseScrolledEvent`
- Gamepad events: `GamepadButtonPressedEvent`, `GamepadConnectedEvent`

### [Entity Component System](../core/ecs.md)
Data-oriented architecture for game entities.

```csharp
var entity = registry.CreateEntity();
var transform = registry.AddComponent<TransformComponent>(entity);
var renderer = registry.AddComponent<MeshRendererComponent>(entity);
```

**Key Classes:**
- `Entity` - Entity identifier
- `Component` - Component base class
- `ECSRegistry` - Entity and component management

**Built-in Components:**
- `TransformComponent` - Position, rotation, scale
- `MeshRendererComponent` - Mesh and material
- `CameraComponent` - Camera properties
- `InventoryComponent` - Entity inventory

## Input Systems

### [Input Handling](../game-dev/input-handling.md)
Keyboard, mouse, and gamepad input.

```csharp
// Polling
if (Input.IsKeyPressed(Key.W))
    MoveForward();

// Gamepad
if (GamepadInput.IsButtonPressed(ButtonName.A))
    Jump();

var leftStick = GamepadInput.GetLeftStick();
```

**Key Classes:**
- `Input` - Keyboard and mouse polling
- `GamepadInput` - Controller input

## Scene Management

### [Scenes](../game-dev/scene-management.md)
Scene and entity management.

```csharp
var scene = new Scene("Level 1");
var player = scene.CreateEntity("Player");

var transform = scene.Registry.AddComponent<TransformComponent>(player);
transform.Position = new Vector3(0, 1, 0);
```

**Key Classes:**
- `Scene` - Scene container
- `Camera` - Camera system

## Game Systems

### [Inventory System](../systems/inventory.md)
Item and inventory management.

```csharp
var inventory = new Inventory(20);
var sword = new SimpleItem("game:sword", "Iron Sword");
inventory.AddItem(sword, 1);
```

**Key Classes:**
- `Item` - Base item class
- `ItemStack` - Stackable items
- `Inventory` - Inventory container
- `ItemRarity` - Rarity levels

### [Block System](../systems/blocks.md)
Voxel block system with properties and behaviors.

```csharp
var stone = new Block
{
    Id = new ResourceKey("game", "stone"),
    Name = "Stone",
    Hardness = 1.5f,
    IsSolid = true
};
BlockRegistry.Instance.Register(stone);
```

**Key Classes:**
- `Block` - Base block class
- `BlockRegistry` - Block management

### [Crafting System](../systems/crafting.md)
Recipe system for item crafting.

```csharp
var recipe = new ShapelessRecipe
{
    Id = new ResourceKey("game", "torch"),
    Ingredients = new() { new Ingredient("game:stick", 1) },
    Result = new RecipeResult("game:torch", 4)
};
RecipeRegistry.Instance.Register(recipe);
```

**Key Classes:**
- `Recipe` - Base recipe class
- `ShapelessRecipe` - Order-independent crafting
- `ShapedRecipe` - Pattern-based crafting
- `SmeltingRecipe` - Furnace recipes
- `RecipeRegistry` - Recipe management

### [Dimension System](../systems/dimensions.md)
Multi-world system with custom properties.

```csharp
var nether = new Dimension
{
    Id = new ResourceKey("game", "nether"),
    Name = "The Nether",
    CoordinateScale = 8.0f,
    Gravity = 1.0f
};
DimensionRegistry.Instance.Register(nether);
```

**Key Classes:**
- `Dimension` - Dimension/world class
- `DimensionRegistry` - Dimension management

## Graphics

### [Vulkan Integration](../graphics/vulkan.md)
Low-level Vulkan rendering API.

```csharp
var vulkanContext = Application.Instance.VulkanContext;
// Vulkan device, queues, swapchain
```

**Key Classes:**
- `VulkanContext` - Vulkan initialization
- `VulkanSwapchain` - Presentation

### [Rendering Pipeline](../graphics/rendering-pipeline.md)
Graphics pipeline (in development).

### [Materials](../graphics/materials.md)
Material system for rendering (planned).

## Modding

### [Mod System](../modding/creating-mods.md)
Create mods to extend the engine.

```csharp
public class MyMod : Mod
{
    public MyMod()
    {
        Info = new ModInfo
        {
            ModId = "mymod",
            Name = "My Mod",
            Version = "1.0.0"
        };
    }

    public override void OnInitialize()
    {
        ModAPI.RegisterBlock(new CustomBlock());
        ModAPI.RegisterItem(new CustomItem());
    }
}
```

**Key Classes:**
- `Mod` - Base mod class
- `ModInfo` - Mod metadata
- `ModLoader` - Mod loading
- `ModAPI` - Helper API

## Utilities

### Math Helpers

```csharp
// Interpolation
float value = MathHelper.Lerp(0f, 10f, 0.5f);
float smooth = MathHelper.SmoothStep(0f, 10f, 0.5f);

// Vector operations
var direction = VectorHelper.Direction(from, to);
var rotated = VectorHelper.RotateAround(point, pivot, axis, angle);

// Quaternion
var rotation = QuaternionHelper.FromEuler(eulerAngles);
var lookRot = QuaternionHelper.LookRotation(forward, up);

// Random
float random = RandomHelper.Range(0f, 100f);
var randomDir = RandomHelper.OnUnitSphere();
```

**Key Classes:**
- `MathHelper` - Math utilities
- `VectorHelper` - Vector operations
- `QuaternionHelper` - Rotation utilities
- `RandomHelper` - Random generation

## Quick Reference by Task

### Creating a Game

```csharp
var app = new Application("My Game", 1280, 720);
app.PushLayer(new GameLayer());
app.Run();
```

### Creating Entities

```csharp
var scene = Application.Instance.ActiveScene;
var entity = scene.CreateEntity("Player");
var transform = scene.Registry.AddComponent<TransformComponent>(entity);
```

### Handling Input

```csharp
// Polling
if (Input.IsKeyPressed(Key.W))
    MoveForward();

// Events
if (e is KeyPressedEvent keyEvent)
    HandleKey(keyEvent.KeyCode);
```

### Loading Resources

```csharp
var resourceManager = Application.Instance.ResourceManager;
var texture = resourceManager.Load<Texture>("texture.png");
```

### Creating a Mod

```csharp
public class MyMod : Mod
{
    public override void OnInitialize()
    {
        ModAPI.RegisterBlock(new CustomBlock());
    }
}
```

## Platform Support

Evoker Engine supports:
- ✅ Windows (Win32)
- ✅ Linux (XCB)
- ✅ macOS (Metal)
- ✅ iOS (Metal)
- ✅ Android

## Requirements

- .NET 9.0 or later
- Vulkan 1.2 or later
- Compatible GPU with Vulkan support

## See Also

- [README](../../README.md) - Project overview
- [Getting Started](../getting-started/installation.md) - Installation guide
- [Game Development](../game-dev/overview.md) - Game development guides
