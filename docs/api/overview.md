# API Overview

Complete API documentation is available in two formats:

## Interactive API Documentation

For detailed API reference with full type information, see the **[DocFX API Documentation](../api/index.html)**.

The DocFX documentation provides:
- Complete type hierarchy
- Method signatures and parameters
- Property documentation
- Code examples
- Cross-references between types

## Quick Reference

### Core Namespaces

#### EvokerEngine.Core
Main application framework and engine systems.

**Key Classes:**
- `Application` - Main application class
- `Layer` - Base class for game layers
- `Logger` - Logging system
- `Time` - Time management and delta time
- `Platform` - Platform detection utilities

#### EvokerEngine.Events
Event system for handling input and window events.

**Key Classes:**
- `Event` - Base event class
- `KeyPressedEvent`, `KeyReleasedEvent` - Keyboard events
- `MouseButtonPressedEvent`, `MouseMovedEvent` - Mouse events
- `GamepadButtonPressedEvent` - Gamepad events

#### EvokerEngine.Input
Input polling for keyboard, mouse, and gamepad.

**Key Classes:**
- `Input` - Keyboard and mouse input
- `GamepadInput` - Gamepad/controller input

#### EvokerEngine.Graphics
Vulkan rendering system.

**Key Classes:**
- `VulkanContext` - Vulkan device and context management
- `VulkanSwapchain` - Swapchain for presenting frames

#### EvokerEngine.ECS
Entity Component System.

**Key Classes:**
- `Entity` - Game object entity
- `Component` - Base component class
- `ECSRegistry` - Entity and component management
- `TransformComponent`, `MeshRendererComponent`, `CameraComponent` - Built-in components

#### EvokerEngine.Scene
Scene and camera management.

**Key Classes:**
- `Scene` - Scene container for entities
- `Camera` - Camera system

#### EvokerEngine.Inventory
Item and inventory system.

**Key Classes:**
- `Item` - Base item class
- `ItemStack` - Stackable items
- `Inventory` - Inventory management
- `ItemRarity` - Item rarity enumeration

#### EvokerEngine.Blocks
Block system for voxel-based games.

**Key Classes:**
- `Block` - Base block class
- `BlockRegistry` - Block registration and management

#### EvokerEngine.Crafting
Recipe and crafting system.

**Key Classes:**
- `Recipe` - Base recipe class
- `ShapelessRecipe`, `ShapedRecipe`, `SmeltingRecipe` - Recipe types
- `RecipeRegistry` - Recipe management
- `Ingredient`, `RecipeResult` - Recipe components

#### EvokerEngine.World
Dimension and world management.

**Key Classes:**
- `Dimension` - World/dimension class
- `DimensionRegistry` - Dimension management
- `DimensionType` - Dimension type enumeration

#### EvokerEngine.Modding
Modding system.

**Key Classes:**
- `Mod` - Base mod class
- `ModLoader` - Mod loading and management
- `ModAPI` - Helper API for mods
- `ModInfo` - Mod metadata

#### EvokerEngine.Resources
Resource management and loading.

**Key Classes:**
- `ResourceManager` - Resource loading and caching
- `ResourceKey` - Namespace:key identifier system

#### EvokerEngine.Utilities
Utility classes and helpers.

**Key Classes:**
- `MathHelper` - Math utilities
- `VectorHelper` - Vector operations
- `QuaternionHelper` - Quaternion operations
- `RandomHelper` - Random number utilities

## Common Patterns

### Application Setup

```csharp
var app = new Application("Game Title", 1280, 720);
app.PushLayer(new MyGameLayer());
app.Run();
```

### Layer Implementation

```csharp
public class MyGameLayer : Layer
{
    public MyGameLayer() : base("My Layer") { }
    
    public override void OnAttach() { }
    public override void OnUpdate(float deltaTime) { }
    public override void OnRender() { }
    public override void OnEvent(Event e) { }
    public override void OnDetach() { }
}
```

### Entity Component System

```csharp
var scene = Application.Instance.ActiveScene;
var entity = scene.CreateEntity("Player");
var transform = scene.Registry.AddComponent<TransformComponent>(entity);
transform.Position = new Vector3(0, 0, 0);
```

### Input Handling

```csharp
// Polling
if (Input.IsKeyPressed(Key.W))
    MoveForward(deltaTime);

// Events
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent)
        HandleKeyPress(keyEvent.KeyCode);
}
```

### Resource Management

```csharp
var resourceManager = Application.Instance.ResourceManager;
var texture = resourceManager.Load<Texture>("my_texture");
```

## Next Steps

- **[View Full API Documentation](../api/index.html)** - Complete DocFX API reference
- [Game Development Guide](../game-dev/overview.md) - Learn to use the API
- [Examples](https://github.com/evokerking1/Evoker-Engine/tree/main/EvokerEngine.Demo) - Sample code
