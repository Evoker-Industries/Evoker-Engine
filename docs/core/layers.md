# Layer System

The Layer system in Evoker Engine provides a modular way to organize game logic, rendering, and UI. Layers allow you to separate concerns and control the order of updates and rendering.

## Overview

Layers are stackable components that:

- Organize game logic into manageable modules
- Control update and render order
- Handle events in a prioritized manner
- Can be pushed, popped, and reordered dynamically
- Support overlays (always-on-top layers)

## Layer Base Class

```csharp
public abstract class Layer
{
    public string Name { get; protected set; }
    
    protected Layer(string name = "Layer");
    
    public virtual void OnAttach() { }
    public virtual void OnDetach() { }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnRender() { }
    public virtual void OnEvent(Event e) { }
}
```

## Creating a Custom Layer

```csharp
using EvokerEngine.Core;
using Silk.NET.Input;

public class GameLayer : Layer
{
    public GameLayer() : base("Game Layer") { }

    public override void OnAttach()
    {
        // Called once when layer is added
        Logger.Info($"{Name} attached");
    }

    public override void OnDetach()
    {
        // Called once when layer is removed
        Logger.Info($"{Name} detached");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Called every frame
        // deltaTime is in seconds
    }

    public override void OnRender()
    {
        // Called every frame for rendering
    }

    public override void OnEvent(Event e)
    {
        // Handle input and window events
        if (e is KeyPressedEvent keyEvent)
        {
            Logger.Info($"Key pressed: {keyEvent.KeyCode}");
        }
    }
}
```

## Layer Lifecycle

### 1. OnAttach()

Called when the layer is first added to the layer stack.

**Use cases:**
- Initialize layer-specific resources
- Create entities and components
- Load assets
- Set up state

```csharp
public override void OnAttach()
{
    var scene = Application.Instance.ActiveScene;
    _player = scene.CreateEntity("Player");
    _camera = scene.CreateEntity("Camera");
}
```

### 2. OnUpdate(float deltaTime)

Called every frame to update game logic.

**Parameters:**
- `deltaTime`: Time since last frame in seconds

**Use cases:**
- Update game logic
- Handle input polling
- Update animations
- Physics calculations

```csharp
public override void OnUpdate(float deltaTime)
{
    // Move player
    if (Input.IsKeyPressed(Key.W))
        _playerPos.Y += 100f * deltaTime;
    
    // Update physics
    _physicsWorld.Step(deltaTime);
}
```

### 3. OnRender()

Called every frame for rendering operations.

**Use cases:**
- Render game objects
- Draw debug information
- Submit rendering commands

```csharp
public override void OnRender()
{
    // Render player
    _renderer.DrawSprite(_playerTexture, _playerPos);
}
```

### 4. OnEvent(Event e)

Called when an event occurs (input, window events).

**Parameters:**
- `e`: Event object (keyboard, mouse, window, etc.)

**Use cases:**
- Handle keyboard/mouse input
- Respond to window resize
- Process gamepad input

```csharp
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent)
    {
        if (keyEvent.KeyCode == Key.Space)
        {
            _player.Jump();
            e.Handled = true; // Stop event propagation
        }
    }
}
```

### 5. OnDetach()

Called when the layer is removed from the layer stack.

**Use cases:**
- Clean up resources
- Save state
- Unload assets

```csharp
public override void OnDetach()
{
    _resourceManager.Unload(_playerTexture);
    SaveGameState();
}
```

## Layer Stack

The LayerStack manages all layers in the application.

### Adding Layers

```csharp
// Regular layers (processed first)
app.PushLayer(new GameLayer());
app.PushLayer(new PhysicsLayer());
app.PushLayer(new AudioLayer());

// Overlays (processed last, rendered on top)
app.PushOverlay(new ImGuiLayer());
app.PushOverlay(new DebugOverlay());
```

### Layer Order

Layers are processed in the order they're added:

```
┌─────────────────┐
│  Debug Overlay  │  ← Overlay (top)
├─────────────────┤
│   ImGui Layer   │  ← Overlay
├─────────────────┤
│   Audio Layer   │  ← Regular layer
├─────────────────┤
│  Physics Layer  │  ← Regular layer
├─────────────────┤
│   Game Layer    │  ← Regular layer (bottom)
└─────────────────┘
```

**Update order:** Bottom to top (Game → Physics → Audio → ImGui → Debug)

**Render order:** Bottom to top (Game → Physics → Audio → ImGui → Debug)

**Event order:** Top to bottom (Debug → ImGui → Audio → Physics → Game)

## Event Handling and Propagation

Events are dispatched to layers in **reverse order** (top to bottom), allowing overlays to handle events first.

### Stopping Event Propagation

Set `e.Handled = true` to prevent the event from reaching lower layers:

```csharp
public override void OnEvent(Event e)
{
    if (e is MouseButtonPressedEvent mouseEvent)
    {
        if (IsMouseOverUI(mouseEvent))
        {
            // UI handled the click, don't pass to game
            e.Handled = true;
        }
    }
}
```

### Example: UI Layer Blocking Game Input

```csharp
class UILayer : Layer
{
    public override void OnEvent(Event e)
    {
        if (e is MouseButtonPressedEvent)
        {
            if (_menuOpen)
            {
                // Menu is open, block game input
                e.Handled = true;
            }
        }
    }
}

class GameLayer : Layer
{
    public override void OnEvent(Event e)
    {
        if (e is MouseButtonPressedEvent mouseEvent)
        {
            // Only runs if UILayer didn't handle it
            if (!e.Handled)
            {
                FireWeapon();
            }
        }
    }
}
```

## Common Layer Patterns

### Game Layer

Main gameplay logic and entities.

```csharp
class GameLayer : Layer
{
    private Scene _scene;
    private List<Entity> _enemies;

    public override void OnAttach()
    {
        _scene = Application.Instance.ActiveScene;
        SpawnPlayer();
        SpawnEnemies();
    }

    public override void OnUpdate(float deltaTime)
    {
        UpdatePlayer(deltaTime);
        UpdateEnemies(deltaTime);
        CheckCollisions();
    }

    public override void OnRender()
    {
        RenderEntities();
    }
}
```

### Physics Layer

Physics simulation and collision detection.

```csharp
class PhysicsLayer : Layer
{
    private PhysicsWorld _world;

    public override void OnAttach()
    {
        _world = new PhysicsWorld();
    }

    public override void OnUpdate(float deltaTime)
    {
        _world.Step(deltaTime);
        ApplyPhysicsToEntities();
    }
}
```

### UI Layer

User interface rendering and interaction.

```csharp
class UILayer : Layer
{
    private bool _menuOpen;

    public override void OnRender()
    {
        if (_menuOpen)
        {
            RenderMenu();
        }
        RenderHUD();
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Escape)
            {
                _menuOpen = !_menuOpen;
                e.Handled = true;
            }
        }
    }
}
```

### Debug Overlay

Debug information and development tools.

```csharp
class DebugOverlay : Layer
{
    private bool _showDebug = true;

    public override void OnRender()
    {
        if (_showDebug)
        {
            RenderFPS();
            RenderPerformanceStats();
            RenderEntityCount();
        }
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.F3)
            {
                _showDebug = !_showDebug;
            }
        }
    }
}
```

## Advanced Usage

### Dynamic Layer Management

You can add and remove layers at runtime:

```csharp
class GameManager : Layer
{
    private Layer? _pauseMenuLayer;

    public void OpenPauseMenu()
    {
        _pauseMenuLayer = new PauseMenuLayer();
        Application.Instance.PushOverlay(_pauseMenuLayer);
    }

    public void ClosePauseMenu()
    {
        if (_pauseMenuLayer != null)
        {
            // Note: PopOverlay not exposed in base Application
            // Typically handle via enable/disable flags
            _pauseMenuLayer = null;
        }
    }
}
```

### Layer Communication

Layers can communicate through events or shared state:

```csharp
class GameLayer : Layer
{
    public int Score { get; set; }
    
    public override void OnUpdate(float deltaTime)
    {
        if (EnemyKilled())
        {
            Score += 100;
        }
    }
}

class UILayer : Layer
{
    private GameLayer? _gameLayer;

    public override void OnAttach()
    {
        // Find game layer
        // In practice, use dependency injection or events
    }

    public override void OnRender()
    {
        if (_gameLayer != null)
        {
            RenderScore(_gameLayer.Score);
        }
    }
}
```

## Best Practices

### ✅ Do's

- Keep layers focused on a single responsibility
- Use overlays for UI and debug information
- Set `e.Handled = true` to stop event propagation
- Clean up resources in `OnDetach()`
- Use descriptive layer names for debugging

### ❌ Don'ts

- Don't make layers depend on each other directly
- Don't store large amounts of state in layers
- Don't perform heavy computations in `OnRender()`
- Don't forget to handle layer removal/cleanup

## Performance Tips

1. **Minimize layer count** - Each layer has overhead
2. **Early exit in OnUpdate** - Skip inactive layers
3. **Batch rendering** - Combine draw calls when possible
4. **Profile layer performance** - Identify bottlenecks

```csharp
public override void OnUpdate(float deltaTime)
{
    if (!_isActive) return; // Early exit
    
    // Update logic
}
```

## Complete Example

```csharp
using EvokerEngine.Core;
using Silk.NET.Input;
using System.Numerics;

class MyGame
{
    static void Main()
    {
        var app = new Application("Layer Example", 1280, 720);
        app.PushLayer(new GameLayer());
        app.PushLayer(new PhysicsLayer());
        app.PushOverlay(new UILayer());
        app.PushOverlay(new DebugOverlay());
        app.Run();
    }
}

class GameLayer : Layer
{
    private Entity _player;

    public GameLayer() : base("Game") { }

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;
        _player = scene.CreateEntity("Player");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Game logic
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

## See Also

- [Application](application.md) - Application lifecycle
- [Events](events.md) - Event system
- [ECS](ecs.md) - Entity Component System
