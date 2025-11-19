# Welcome to Evoker Engine

[![CI/CD Pipeline](https://github.com/evokerking1/Evoker-Engine/actions/workflows/ci.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/ci.yml)
[![PR Validation](https://github.com/evokerking1/Evoker-Engine/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/pr-validation.yml)
[![Security](https://github.com/evokerking1/Evoker-Engine/actions/workflows/dependency-security.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/dependency-security.yml)

**Evoker Engine** is a complete C# game engine with Vulkan rendering support using Silk.NET. It provides a powerful, modern framework for creating 2D and 3D games with extensive modding support.

## Key Features

### 🎮 Complete Game Engine
- Application framework with window management and main loop
- Layer-based architecture for organizing game logic
- Comprehensive event system for input and window events
- Advanced time management with delta time and FPS tracking

### 🎨 Modern Graphics
- Full Vulkan integration using Silk.NET
- Multi-platform support (Windows, Linux, macOS, iOS, Android)
- Automatic platform-specific Vulkan surface extensions
- Swapchain management and rendering pipeline

### 🕹️ Rich Input System
- Keyboard and mouse input with polling support
- Complete gamepad/controller support with deadzone handling
- Hot-plug support for controllers
- Multiple controller support

### 🏗️ Entity Component System (ECS)
- Efficient entity and component management
- Built-in components for transforms, rendering, cameras, and inventory
- Scene management system
- Camera controls and projection matrices

### 🎯 Game Systems
- **Inventory System**: Items, stacks, weight limits, rarity system
- **Block System**: Minecraft-style blocks with properties and events
- **Crafting System**: Shapeless, shaped, and smelting recipes
- **Dimension System**: Multiple worlds with custom properties

### 🔧 Powerful Modding
- Easy mod creation with lifecycle hooks
- Automatic mod discovery and loading
- Dependency resolution
- Comprehensive ModAPI for content registration

## Quick Start

```csharp
using EvokerEngine.Core;
using Silk.NET.Input;

// Create application
var app = new Application("My Game", 1280, 720);

// Add your game layer
app.PushLayer(new MyGameLayer());

// Run the application
app.Run();

// Custom layer implementation
class MyGameLayer : Layer
{
    public MyGameLayer() : base("Game Layer") { }

    public override void OnAttach()
    {
        Logger.Info("Game initialized!");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Update game logic
    }

    public override void OnRender()
    {
        // Render game objects
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Escape)
            {
                Application.Instance.Close();
            }
        }
    }
}
```

## Getting Started

<div class="grid cards" markdown>

-   :material-clock-fast:{ .lg .middle } __Quick Start__

    ---

    Get up and running in minutes with our quick start guide.

    [:octicons-arrow-right-24: Quick Start](getting-started/quick-start.md)

-   :material-gamepad:{ .lg .middle } __Game Development__

    ---

    Learn how to create 2D and 3D games with Evoker Engine.

    [:octicons-arrow-right-24: Game Development](game-dev/overview.md)

-   :material-puzzle:{ .lg .middle } __Modding__

    ---

    Create mods and extend the engine with custom content.

    [:octicons-arrow-right-24: Modding Guide](modding/creating-mods.md)

-   :material-api:{ .lg .middle } __API Reference__

    ---

    Explore the complete API documentation.

    [:octicons-arrow-right-24: API Docs](api/overview.md)

</div>

## Platform Support

Evoker Engine runs on multiple platforms with unified API:

- ✅ **Windows** - Full support with Win32 Vulkan surface
- ✅ **Linux** - Full support with XCB Vulkan surface
- ✅ **macOS** - Full support with Metal Vulkan surface
- ✅ **iOS** - Mobile support with Metal Vulkan surface
- ✅ **Android** - Mobile support with Android Vulkan surface

## Architecture

The engine is organized into clean, modular namespaces:

```
EvokerEngine.Core/
├── Core/              # Application, layers, logging, time
├── Graphics/          # Vulkan rendering system
├── Input/             # Keyboard, mouse, gamepad input
├── Events/            # Event system
├── ECS/               # Entity Component System
├── Scene/             # Scene and camera management
├── Resources/         # Resource management
├── Inventory/         # Item and inventory system
├── Blocks/            # Block system
├── Crafting/          # Recipe system
├── Modding/           # Mod support
└── World/             # Dimension system
```

## Community & Support

- 📖 [Documentation](https://evokerking1.github.io/Evoker-Engine/)
- 🐛 [Report Issues](https://github.com/evokerking1/Evoker-Engine/issues)
- 💬 [Discussions](https://github.com/evokerking1/Evoker-Engine/discussions)
- 📝 [Contributing](contributing.md)

## License

This project is open source.
