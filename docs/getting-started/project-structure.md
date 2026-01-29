---
tags:
  - getting-started
  - architecture
---

# Project Structure

Understanding the Evoker Engine project structure will help you navigate the codebase and organize your own projects.

## Repository Structure

```
Evoker-Engine/
├── EvokerEngine.Core/       # Core engine library
├── EvokerEngine.Demo/       # Example application
├── EvokerEngine.Tests/      # Unit tests
├── docs/                    # Documentation source
├── .github/                 # GitHub workflows
├── README.md
├── mkdocs.yml              # Documentation configuration
└── EvokerEngine.sln        # Solution file
```

## EvokerEngine.Core Structure

The core library is organized into namespaces:

```
EvokerEngine.Core/
├── Core/                    # Application framework
│   ├── Application.cs       # Main application class
│   ├── Layer.cs            # Layer base class
│   ├── LayerStack.cs       # Layer management
│   ├── Logger.cs           # Logging system
│   ├── Time.cs             # Time management
│   └── Platform.cs         # Platform utilities
├── Graphics/               # Rendering system
│   ├── VulkanContext.cs    # Vulkan initialization
│   └── VulkanSwapchain.cs  # Swapchain management
├── Input/                  # Input handling
│   ├── Input.cs            # Keyboard/mouse input
│   └── GamepadInput.cs     # Gamepad support
├── Events/                 # Event system
│   └── Event.cs            # Event definitions
├── ECS/                    # Entity Component System
│   ├── Entity.cs           # Entity definition
│   ├── Component.cs        # Component base
│   ├── Components.cs       # Built-in components
│   └── ECSRegistry.cs      # ECS management
├── Scene/                  # Scene management
│   ├── Scene.cs            # Scene definition
│   └── Camera.cs           # Camera system
├── Resources/              # Resource management
│   └── ResourceManager.cs  # Resource loading
├── Inventory/              # Inventory system
│   ├── Item.cs             # Item definitions
│   ├── ItemStack.cs        # Stackable items
│   └── Inventory.cs        # Inventory management
├── Blocks/                 # Block system
│   ├── Block.cs            # Block base class
│   └── BlockRegistry.cs    # Block registration
├── Crafting/               # Crafting system
│   ├── Recipe.cs           # Recipe types
│   └── RecipeRegistry.cs   # Recipe management
├── Modding/                # Modding support
│   ├── Mod.cs              # Mod base class
│   ├── ModLoader.cs        # Mod loading
│   └── ModAPI.cs           # Helper API
└── World/                  # World/dimension system
    ├── Dimension.cs        # Dimension class
    └── DimensionRegistry.cs # Dimension management
```

## Your Game Project Structure

Recommended structure for your game:

```
MyGame/
├── Content/                # Game assets
│   ├── Textures/
│   ├── Models/
│   ├── Shaders/
│   └── Audio/
├── Layers/                 # Game layers
│   ├── GameLayer.cs
│   ├── UILayer.cs
│   └── MenuLayer.cs
├── Entities/               # Game entities
│   ├── Player.cs
│   ├── Enemy.cs
│   └── Items/
├── Systems/                # Game systems
│   ├── CombatSystem.cs
│   └── QuestSystem.cs
├── Mods/                   # Mod DLLs
│   └── ExampleMod.dll
└── Program.cs              # Entry point
```

## Key Files

### Application Entry Point

`Program.cs` - Your game's main entry point:

```csharp
using EvokerEngine.Core;

var app = new Application("My Game", 1280, 720);
app.PushLayer(new GameLayer());
app.Run();
```

### Layer Files

Organize game logic into layers:

```csharp
// Layers/GameLayer.cs
public class GameLayer : Layer
{
    public override void OnAttach() { }
    public override void OnUpdate(float dt) { }
    public override void OnRender() { }
}
```

### Project File

`.csproj` - Project configuration:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\EvokerEngine.Core\EvokerEngine.Core.csproj" />
  </ItemGroup>
</Project>
```

## Next Steps

- [Quick Start Guide](quick-start.md)
- [Game Development Overview](../game-dev/overview.md)
