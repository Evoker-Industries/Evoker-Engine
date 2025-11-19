# Game Development Overview

Evoker Engine provides everything you need to create both 2D and 3D games. This guide gives you an overview of the game development workflow.

## Game Development Workflow

### 1. Project Setup

```bash
# Create your game project
dotnet new console -n MyGame
cd MyGame

# Add Evoker Engine reference
dotnet add reference path/to/EvokerEngine.Core/EvokerEngine.Core.csproj
```

### 2. Choose Your Game Type

=== "2D Games"
    Perfect for:
    - Platformers
    - Top-down games
    - Side-scrollers
    - Puzzle games
    - Retro-style games

    [Learn More →](2d-games.md){ .md-button }

=== "3D Games"
    Perfect for:
    - First-person games
    - Third-person adventures
    - Racing games
    - Simulation games
    - Open-world games

    [Learn More →](3d-games.md){ .md-button }

### 3. Core Development Concepts

#### Application Structure

```csharp
// Create application
var app = new Application("My Game", 1280, 720);

// Add your layers
app.PushLayer(new GameLayer());
app.PushLayer(new UILayer());

// Run
app.Run();
```

#### Layer System

Layers organize your game into logical sections:

```csharp
class GameLayer : Layer
{
    public override void OnAttach() { }       // Initialize
    public override void OnUpdate(float dt) { } // Update logic
    public override void OnRender() { }       // Draw graphics
    public override void OnEvent(Event e) { } // Handle input
    public override void OnDetach() { }       // Cleanup
}
```

#### Entity Component System

Create game objects using entities and components:

```csharp
var scene = Application.Instance.ActiveScene;

// Create entity
var player = scene.CreateEntity("Player");

// Add components
var transform = scene.Registry.AddComponent<TransformComponent>(player);
transform.Position = new Vector3(0, 0, 0);

var renderer = scene.Registry.AddComponent<MeshRendererComponent>(player);
renderer.MeshId = "player_mesh";
```

## Game Systems

### Input Handling

Evoker Engine supports multiple input methods:

```csharp
// Keyboard
if (Input.IsKeyPressed(Key.W))
    MoveForward();

// Mouse
var mousePos = Input.GetMousePosition();
if (Input.IsMouseButtonPressed(MouseButton.Left))
    Shoot();

// Gamepad
if (GamepadInput.IsGamepadConnected(0))
{
    var stick = GamepadInput.GetLeftStick();
    Move(stick);
}
```

[Input Handling Guide →](input-handling.md)

### Scene Management

Organize your game world with scenes:

```csharp
// Access active scene
var scene = Application.Instance.ActiveScene;

// Create entities
var player = scene.CreateEntity("Player");
var enemy = scene.CreateEntity("Enemy");

// Camera setup
var camera = scene.CreateEntity("Camera");
var cam = scene.Registry.AddComponent<CameraComponent>(camera);
```

[Scene Management Guide →](scene-management.md)

### Resource Management

Load and manage game assets:

```csharp
var resourceManager = Application.Instance.ResourceManager;

// Load resources
var texture = resourceManager.Load<Texture>("player_texture");
var mesh = resourceManager.Load<Mesh>("player_model");
var shader = resourceManager.Load<Shader>("default_shader");
```

## Built-in Game Systems

### Inventory System

Create items and manage inventories:

```csharp
// Create an item
var sword = new Item
{
    Id = new ResourceKey("mygame", "iron_sword"),
    Name = "Iron Sword",
    MaxStackSize = 1,
    Rarity = ItemRarity.Uncommon
};

// Create inventory
var inventory = new Inventory(maxSlots: 20, maxWeight: 100f);
inventory.AddItem(sword, 1);
```

[Inventory System Guide →](../systems/inventory.md)

### Block System

For voxel-based games (like Minecraft):

```csharp
// Register a block
var stone = new Block
{
    Id = new ResourceKey("mygame", "stone"),
    Name = "Stone",
    Hardness = 2.0f,
    IsSolid = true
};
BlockRegistry.Instance.Register(stone);
```

[Block System Guide →](../systems/blocks.md)

### Crafting System

Add crafting mechanics:

```csharp
// Shapeless recipe
var recipe = new ShapelessRecipe
{
    Id = new ResourceKey("mygame", "iron_sword_recipe"),
    Name = "Iron Sword"
};
recipe.Ingredients.Add(new Ingredient("mygame:iron_ingot", 3));
recipe.Ingredients.Add(new Ingredient("mygame:stick", 1));
recipe.Result = new RecipeResult("mygame:iron_sword", 1);

RecipeRegistry.Instance.Register(recipe);
```

[Crafting System Guide →](../systems/crafting.md)

### Dimension System

Create multiple worlds:

```csharp
var dimension = new Dimension
{
    Id = new ResourceKey("mygame", "nether"),
    Name = "The Nether",
    HasSky = true,
    MinHeight = 0,
    MaxHeight = 128,
    Gravity = 0.8f
};
DimensionRegistry.Instance.Register(dimension);
```

[Dimension System Guide →](../systems/dimensions.md)

## Development Best Practices

### Frame-Rate Independence

Always use delta time for movement:

```csharp
public override void OnUpdate(float deltaTime)
{
    // Good: Frame-rate independent
    position += velocity * deltaTime;
    
    // Bad: Tied to frame rate
    position += velocity;
}
```

### Component Organization

Keep components focused and reusable:

```csharp
// Good: Single responsibility
public class HealthComponent : Component
{
    public float Health { get; set; }
    public float MaxHealth { get; set; }
}

public class DamageComponent : Component
{
    public float Damage { get; set; }
    public float Range { get; set; }
}
```

### Event vs Polling

Choose the right input method:

```csharp
// Events: For immediate one-time actions
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent key && key.KeyCode == Key.Space)
        Jump(); // Execute once
}

// Polling: For continuous actions
public override void OnUpdate(float deltaTime)
{
    if (Input.IsKeyPressed(Key.W))
        MoveForward(deltaTime); // Smooth movement
}
```

### Resource Cleanup

Clean up resources properly:

```csharp
public override void OnDetach()
{
    // Unload resources
    resourceManager.Unload("temporary_texture");
    
    // Clear references
    entities.Clear();
    
    Logger.Info("Layer cleaned up");
}
```

## Performance Tips

### Entity Management

```csharp
// Use object pooling for frequently created entities
private Queue<Entity> _bulletPool = new();

Entity GetBullet()
{
    if (_bulletPool.Count > 0)
        return _bulletPool.Dequeue();
    return scene.CreateEntity("Bullet");
}

void ReturnBullet(Entity bullet)
{
    _bulletPool.Enqueue(bullet);
}
```

### Component Queries

```csharp
// Cache component queries
private List<Entity> _renderables;

public override void OnAttach()
{
    _renderables = scene.Registry
        .GetEntitiesWithComponent<MeshRendererComponent>()
        .ToList();
}
```

### Update Frequency

```csharp
private float _updateTimer = 0f;
private const float UpdateInterval = 0.1f; // 10 times per second

public override void OnUpdate(float deltaTime)
{
    _updateTimer += deltaTime;
    
    if (_updateTimer >= UpdateInterval)
    {
        _updateTimer -= UpdateInterval;
        // Expensive update here
    }
    
    // Fast updates every frame
}
```

## Example: Complete Game Template

Here's a complete game template to get you started:

```csharp
using EvokerEngine.Core;
using EvokerEngine.ECS;
using EvokerEngine.Scene;
using System.Numerics;

var app = new Application("My Game", 1280, 720);
app.PushLayer(new GameLayer());
app.PushLayer(new UILayer());
app.Run();

class GameLayer : Layer
{
    private Scene _scene;
    private Entity _player;
    private List<Entity> _enemies = new();

    public GameLayer() : base("Game") { }

    public override void OnAttach()
    {
        _scene = Application.Instance.ActiveScene;
        CreatePlayer();
        CreateEnemies(5);
        Logger.Info("Game initialized");
    }

    private void CreatePlayer()
    {
        _player = _scene.CreateEntity("Player");
        var transform = _scene.Registry.AddComponent<TransformComponent>(_player);
        transform.Position = Vector3.Zero;
    }

    private void CreateEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var enemy = _scene.CreateEntity($"Enemy_{i}");
            var transform = _scene.Registry.AddComponent<TransformComponent>(enemy);
            transform.Position = new Vector3(i * 2f, 0, 5);
            _enemies.Add(enemy);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        UpdatePlayer(deltaTime);
        UpdateEnemies(deltaTime);
    }

    private void UpdatePlayer(float deltaTime)
    {
        var transform = _scene.Registry.GetComponent<TransformComponent>(_player);
        float speed = 5f * deltaTime;

        if (Input.IsKeyPressed(Silk.NET.Input.Key.W))
            transform.Position += new Vector3(0, 0, -speed);
        if (Input.IsKeyPressed(Silk.NET.Input.Key.S))
            transform.Position += new Vector3(0, 0, speed);
        if (Input.IsKeyPressed(Silk.NET.Input.Key.A))
            transform.Position += new Vector3(-speed, 0, 0);
        if (Input.IsKeyPressed(Silk.NET.Input.Key.D))
            transform.Position += new Vector3(speed, 0, 0);
    }

    private void UpdateEnemies(float deltaTime)
    {
        foreach (var enemy in _enemies)
        {
            var transform = _scene.Registry.GetComponent<TransformComponent>(enemy);
            // AI logic here
        }
    }

    public override void OnRender()
    {
        // Rendering logic
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent key && key.KeyCode == Silk.NET.Input.Key.Escape)
            Application.Instance.Close();
    }

    public override void OnDetach()
    {
        Logger.Info("Game shutting down");
    }
}

class UILayer : Layer
{
    public UILayer() : base("UI") { }

    public override void OnRender()
    {
        // UI rendering
    }
}
```

## Next Steps

Choose your path:

- 📱 [2D Game Development](2d-games.md) - Learn to create 2D games
- 🎮 [3D Game Development](3d-games.md) - Learn to create 3D games
- 🎯 [Input Handling](input-handling.md) - Master input systems
- 🏗️ [Scene Management](scene-management.md) - Organize your game world
- 🔧 [Modding](../modding/creating-mods.md) - Extend the engine

## Additional Resources

- [API Reference](../api/overview.md) - Complete API documentation
- [Core Systems](../core/application.md) - Deep dive into engine systems
- [Graphics](../graphics/vulkan.md) - Rendering and graphics
- [Examples](https://github.com/evokerking1/Evoker-Engine/tree/main/EvokerEngine.Demo) - Sample code
