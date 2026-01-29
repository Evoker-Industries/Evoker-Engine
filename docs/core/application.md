---
tags:
  - coresystem
---

# Application

The `Application` class is the core of Evoker Engine. It manages the main application lifecycle, window creation, rendering loop, and coordinate all major systems.

## Overview

The Application class is a singleton that:

- Creates and manages the application window
- Initializes Vulkan rendering context
- Manages the layer stack for organizing game logic
- Coordinates the update and render loops
- Handles input system initialization
- Manages scenes and resources
- Dispatches events to layers

## Creating an Application

```csharp
using EvokerEngine.Core;

// Create application with default settings (1280x720)
var app = new Application("My Game");

// Or specify custom window size
var app = new Application("My Game", 1920, 1080);

// Push layers to add functionality
app.PushLayer(new GameLayer());
app.PushOverlay(new UILayer());

// Run the application (blocks until window closes)
app.Run();
```

## Application Lifecycle

### 1. Initialization (Constructor)

When you create an Application instance:

1. Sets up the singleton instance
2. Creates the main scene
3. Configures window options for Vulkan
4. Creates the window using Silk.NET
5. Registers event callbacks

### 2. Loading (OnLoad)

Called once when the window is ready:

1. Initializes input systems (keyboard, mouse, gamepad)
2. Sets up input event callbacks
3. Initializes Vulkan context
4. Creates the swapchain
5. Resets the time system
6. Calls `OnAttach()` on all layers

### 3. Update Loop (OnUpdate)

Called every frame:

1. Updates Time system (delta time, FPS)
2. Calls `OnUpdate()` on all layers
3. Updates the active scene
4. Skipped when window is minimized

### 4. Render Loop (OnRender)

Called every frame:

1. Calls `OnRender()` on all layers
2. Performs rendering operations
3. Skipped when window is minimized

### 5. Shutdown (OnClose)

Called when the application closes:

1. Cleans up swapchain
2. Cleans up Vulkan context
3. Unloads all resources
4. Calls `OnDetach()` on all layers

## Properties

### Instance

```csharp
public static Application Instance
```

Gets the singleton Application instance. Throws exception if not created.

### Window

```csharp
public IWindow Window
```

Gets the application window. Used to query window size, title, etc.

### VulkanContext

```csharp
public VulkanContext VulkanContext
```

Gets the Vulkan rendering context for advanced rendering operations.

### ActiveScene

```csharp
public Scene ActiveScene
```

Gets the currently active scene containing entities and components.

### ResourceManager

```csharp
public ResourceManager ResourceManager
```

Gets the resource manager for loading and managing assets.

## Methods

### Run()

```csharp
public void Run()
```

Starts the application main loop. This method blocks until the window is closed.

**Example:**
```csharp
var app = new Application("My Game");
app.PushLayer(new GameLayer());
app.Run(); // Blocks here until window closes
```

### Close()

```csharp
public void Close()
```

Closes the application and exits the main loop.

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent && keyEvent.KeyCode == Key.Escape)
    {
        Application.Instance.Close();
    }
}
```

### PushLayer()

```csharp
public void PushLayer(Layer layer)
```

Adds a layer to the layer stack. Layers are processed in the order they're added.

**Example:**
```csharp
app.PushLayer(new GameLayer());
app.PushLayer(new PhysicsLayer());
```

### PushOverlay()

```csharp
public void PushOverlay(Layer overlay)
```

Adds an overlay layer. Overlays are always processed after regular layers and rendered on top.

**Example:**
```csharp
app.PushOverlay(new ImGuiLayer());
app.PushOverlay(new DebugOverlay());
```

## Event Handling

The Application automatically dispatches events to layers in reverse order (top to bottom). This allows overlays to handle events first.

**Events dispatched:**

- Window events: `WindowResizeEvent`, `WindowCloseEvent`
- Keyboard events: `KeyPressedEvent`, `KeyReleasedEvent`
- Mouse events: `MouseButtonPressedEvent`, `MouseButtonReleasedEvent`, `MouseMovedEvent`, `MouseScrolledEvent`
- Gamepad events: `GamepadButtonPressedEvent`, `GamepadButtonReleasedEvent`

## Complete Example

```csharp
using EvokerEngine.Core;
using Silk.NET.Input;

namespace MyGame;

class Program
{
    static void Main(string[] args)
    {
        var app = new Application("My Awesome Game", 1280, 720);
        app.PushLayer(new GameLayer());
        app.Run();
    }
}

class GameLayer : Layer
{
    private Entity _player;

    public GameLayer() : base("Game Layer") { }

    public override void OnAttach()
    {
        Logger.Info("Game layer attached");
        
        // Create player entity
        var scene = Application.Instance.ActiveScene;
        _player = scene.CreateEntity("Player");
        
        var transform = scene.Registry.AddComponent<TransformComponent>(_player);
        transform.Position = new Vector3(0, 0, 0);
    }

    public override void OnUpdate(float deltaTime)
    {
        var scene = Application.Instance.ActiveScene;
        var transform = scene.Registry.GetComponent<TransformComponent>(_player);
        
        // Move player with WASD
        float speed = 5f * deltaTime;
        if (Input.IsKeyPressed(Key.W))
            transform.Position += new Vector3(0, 0, -speed);
        if (Input.IsKeyPressed(Key.S))
            transform.Position += new Vector3(0, 0, speed);
        if (Input.IsKeyPressed(Key.A))
            transform.Position += new Vector3(-speed, 0, 0);
        if (Input.IsKeyPressed(Key.D))
            transform.Position += new Vector3(speed, 0, 0);
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

    public override void OnDetach()
    {
        Logger.Info("Game layer detached");
    }
}
```

## Best Practices

### ✅ Do's

- Create only one Application instance
- Use `Application.Instance` to access the singleton
- Push layers in the correct order (gameplay first, UI last)
- Handle window resize events to update camera/viewport
- Clean up resources in `OnDetach()`

### ❌ Don'ts

- Don't create multiple Application instances
- Don't block the update/render loops with long operations
- Don't directly access window callbacks (use events instead)
- Don't forget to call `Run()` after setup

## Platform Support

The Application class works seamlessly across all supported platforms:

- **Windows**: Win32 surface, DirectInput
- **Linux**: XCB/Wayland surface, evdev input
- **macOS**: Metal surface, IOKit input
- **iOS**: Metal surface, UIKit input
- **Android**: Android surface, native input

Platform detection is automatic - no special code required!

## See Also

- [Layers](layers.md) - Layer system documentation
- [Events](events.md) - Event system documentation
- [Scene Management](../game-dev/scene-management.md) - Scene and entity management
