---
tags:
  - getting-started
  - tutorial
---

# Quick Start

This guide will help you create your first game with Evoker Engine in just a few minutes.

## Create Your First Application

### Step 1: Create a New Project

```bash
dotnet new console -n MyFirstGame
cd MyFirstGame
dotnet add reference path/to/EvokerEngine.Core/EvokerEngine.Core.csproj
```

### Step 2: Create Your Game

Replace the contents of `Program.cs` with:

```csharp title="Program.cs"
using EvokerEngine.Core;
using EvokerEngine.Events;
using Silk.NET.Input;

// Create and run the application
var app = new Application("My First Game", 1280, 720);
app.PushLayer(new GameLayer());
app.Run();

// Game layer implementation
class GameLayer : Layer
{
    public GameLayer() : base("Game Layer") { }

    public override void OnAttach()
    {
        Logger.Info("Game started!");
    }

    public override void OnUpdate(float deltaTime)
    {
        // This runs every frame
        // deltaTime is the time since last frame in seconds
    }

    public override void OnRender()
    {
        // Rendering code goes here
    }

    public override void OnEvent(Event e)
    {
        // Handle input events
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Escape)
            {
                Logger.Info("Escape pressed - exiting");
                Application.Instance.Close();
            }
        }
    }

    public override void OnDetach()
    {
        Logger.Info("Game shutting down");
    }
}
```

### Step 3: Run Your Game

```bash
dotnet run
```

You should see a window open with the title "My First Game". Press ESC to close it.

## Understanding the Code

Let's break down what each part does:

### The Application

```csharp
var app = new Application("My First Game", 1280, 720);
```

Creates the main application with:
- Window title: "My First Game"
- Resolution: 1280x720
- Initializes Vulkan rendering
- Sets up the event system

### Layers

```csharp
app.PushLayer(new GameLayer());
```

Layers are the building blocks of your game. They receive:
- Update callbacks every frame
- Render callbacks for drawing
- Event notifications for input

You can have multiple layers (UI, game logic, debug overlay, etc.).

### The Game Loop

The engine automatically handles the game loop:

1. **OnUpdate(deltaTime)** - Update game state
2. **OnRender()** - Draw graphics
3. **OnEvent(Event)** - Handle input events

## Add Some Interactivity

Let's make a simple interactive example:

```csharp title="Program.cs"
using EvokerEngine.Core;
using EvokerEngine.Events;
using Silk.NET.Input;

var app = new Application("Interactive Demo", 1280, 720);
app.PushLayer(new InteractiveLayer());
app.Run();

class InteractiveLayer : Layer
{
    private float _rotation = 0f;
    private float _speed = 45f; // degrees per second

    public InteractiveLayer() : base("Interactive Layer") { }

    public override void OnAttach()
    {
        Logger.Info("Use WASD to control speed, Space to reset");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Rotate continuously
        _rotation += _speed * deltaTime;
        if (_rotation > 360f)
            _rotation -= 360f;

        // Check for input (polling)
        if (Input.IsKeyPressed(Key.W))
            _speed += 10f * deltaTime;
        if (Input.IsKeyPressed(Key.S))
            _speed -= 10f * deltaTime;
        if (Input.IsKeyPressed(Key.Space))
            _speed = 45f;

        // Clamp speed
        _speed = Math.Clamp(_speed, -180f, 180f);
    }

    public override void OnRender()
    {
        // Rendering would go here
        // For now, we'll just log periodically
        if (_rotation % 45f < 1f)
        {
            Logger.Info($"Rotation: {_rotation:F1}°, Speed: {_speed:F1}°/s");
        }
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            switch (keyEvent.KeyCode)
            {
                case Key.Escape:
                    Application.Instance.Close();
                    break;
                case Key.R:
                    _rotation = 0f;
                    Logger.Info("Rotation reset!");
                    break;
            }
        }
    }
}
```

Run this and experiment with the controls:
- **W** - Increase rotation speed
- **S** - Decrease rotation speed
- **Space** - Reset to default speed
- **R** - Reset rotation to 0
- **ESC** - Exit

## Working with Entities

Let's create some game objects using the Entity Component System:

```csharp title="Program.cs"
using EvokerEngine.Core;
using EvokerEngine.ECS;
using EvokerEngine.Scene;
using System.Numerics;

var app = new Application("ECS Demo", 1280, 720);
app.PushLayer(new ECSLayer());
app.Run();

class ECSLayer : Layer
{
    private Scene _scene;
    private Entity _player;

    public ECSLayer() : base("ECS Layer") { }

    public override void OnAttach()
    {
        // Get the active scene
        _scene = Application.Instance.ActiveScene;

        // Create a player entity
        _player = _scene.CreateEntity("Player");
        
        // Add a transform component
        var transform = _scene.Registry.AddComponent<TransformComponent>(_player);
        transform.Position = new Vector3(0, 0, 0);
        transform.Rotation = new Vector3(0, 0, 0);
        transform.Scale = Vector3.One;

        Logger.Info($"Created player entity with ID: {_player.Id}");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Get the player's transform
        var transform = _scene.Registry.GetComponent<TransformComponent>(_player);

        // Move with arrow keys
        float moveSpeed = 5f * deltaTime;
        if (Input.IsKeyPressed(Silk.NET.Input.Key.Left))
            transform.Position += new Vector3(-moveSpeed, 0, 0);
        if (Input.IsKeyPressed(Silk.NET.Input.Key.Right))
            transform.Position += new Vector3(moveSpeed, 0, 0);
        if (Input.IsKeyPressed(Silk.NET.Input.Key.Up))
            transform.Position += new Vector3(0, moveSpeed, 0);
        if (Input.IsKeyPressed(Silk.NET.Input.Key.Down))
            transform.Position += new Vector3(0, -moveSpeed, 0);
    }

    public override void OnRender()
    {
        var transform = _scene.Registry.GetComponent<TransformComponent>(_player);
        // In a real game, you'd render the entity here
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent && keyEvent.KeyCode == Silk.NET.Input.Key.Escape)
        {
            var transform = _scene.Registry.GetComponent<TransformComponent>(_player);
            Logger.Info($"Final position: {transform.Position}");
            Application.Instance.Close();
        }
    }
}
```

## Next Steps

Now that you've created your first application, you can:

- 📖 Learn about [2D Game Development](../game-dev/2d-games.md)
- 🎮 Explore [3D Game Development](../game-dev/3d-games.md)
- 🎯 Understand the [ECS System](../core/ecs.md)
- 🎨 Learn about [Graphics and Rendering](../graphics/vulkan.md)
- 🔧 Check out the [Modding System](../modding/creating-mods.md)

## Common Patterns

### Multiple Layers

```csharp
app.PushLayer(new GameLayer());      // Game logic
app.PushLayer(new UILayer());        // User interface
app.PushLayer(new DebugLayer());     // Debug overlay
```

Layers are processed in order (first pushed = first updated).

### Input Handling

```csharp
// Event-based (immediate response)
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent key)
        HandleKeyPress(key.KeyCode);
}

// Polling-based (smooth movement)
public override void OnUpdate(float deltaTime)
{
    if (Input.IsKeyPressed(Key.W))
        MoveForward(deltaTime);
}
```

### Time Management

```csharp
public override void OnUpdate(float deltaTime)
{
    // deltaTime is in seconds
    // Use it to make movement frame-rate independent
    position += velocity * deltaTime;
    
    // Access time properties
    float totalTime = Time.TotalTime;
    float fps = Time.FPS;
    float timeScale = Time.TimeScale; // For slow-motion effects
}
```

## Tips

!!! tip "Use deltaTime"
    Always multiply movement and animations by `deltaTime` to make them frame-rate independent.

!!! tip "Layer Organization"
    Use separate layers for different concerns (game logic, UI, debug) to keep code organized.

!!! tip "Entity Components"
    The ECS system is powerful - learn it early to structure your game properly.

!!! warning "Vulkan Initialization"
    The engine handles Vulkan setup automatically. Don't try to initialize it manually.
