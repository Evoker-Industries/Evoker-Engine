# Entity Component System (ECS)

The Entity Component System (ECS) in Evoker Engine provides a data-oriented architecture for managing game objects. It separates data (Components) from behavior and allows for flexible, composable game entities.

## Overview

The ECS architecture consists of three main concepts:

- **Entities**: Unique identifiers for game objects
- **Components**: Data containers attached to entities
- **Systems**: Logic that operates on entities with specific components

## Core Components

### Entity

An entity is simply a unique identifier (uint ID):

```csharp
public struct Entity : IEquatable<Entity>
{
    public readonly uint Id;
}
```

Entities are lightweight and only serve as a handle to reference components.

### Component

The base class for all components:

```csharp
public abstract class Component
{
    public Entity Entity { get; internal set; }
}
```

Components store data and are attached to entities.

### ECSRegistry

The registry manages all entities and components:

```csharp
public class ECSRegistry
{
    public Entity CreateEntity();
    public void DestroyEntity(Entity entity);
    
    public T AddComponent<T>(Entity entity) where T : Component, new();
    public T? GetComponent<T>(Entity entity) where T : Component;
    public bool HasComponent<T>(Entity entity) where T : Component;
    public void RemoveComponent<T>(Entity entity) where T : Component;
    
    public IEnumerable<T> GetAllComponents<T>() where T : Component;
    public IEnumerable<Entity> GetAllEntities();
}
```

## Creating Entities

### Basic Entity Creation

```csharp
using EvokerEngine.ECS;
using EvokerEngine.Core;

// Get the scene's ECS registry
var scene = Application.Instance.ActiveScene;
var registry = scene.Registry;

// Create an entity
var entity = registry.CreateEntity();

// Add components
var transform = registry.AddComponent<TransformComponent>(entity);
transform.Position = new Vector3(0, 0, 0);
transform.Scale = Vector3.One;
```

### Using Scene Helper

The Scene class provides a convenient method:

```csharp
var scene = Application.Instance.ActiveScene;

// Creates entity and adds a NameComponent
var entity = scene.CreateEntity("MyEntity");
```

## Built-in Components

### TransformComponent

Stores position, rotation, and scale.

```csharp
public class TransformComponent : Component
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }  // Euler angles in degrees
    public Vector3 Scale { get; set; }
    
    public Matrix4x4 GetTransformMatrix();
}
```

**Example:**
```csharp
var transform = registry.AddComponent<TransformComponent>(entity);
transform.Position = new Vector3(5, 0, 10);
transform.Rotation = new Vector3(0, 45, 0);  // Rotate 45° around Y
transform.Scale = new Vector3(2, 2, 2);      // Double size

// Get transform matrix for rendering
var matrix = transform.GetTransformMatrix();
```

### MeshRendererComponent

Defines what mesh and material to render.

```csharp
public class MeshRendererComponent : Component
{
    public int MeshId { get; set; }
    public int MaterialId { get; set; }
}
```

**Example:**
```csharp
var renderer = registry.AddComponent<MeshRendererComponent>(entity);
renderer.MeshId = 0;      // Cube mesh
renderer.MaterialId = 1;  // Red material
```

### CameraComponent

Camera properties for rendering.

```csharp
public class CameraComponent : Component
{
    public float FieldOfView { get; set; }     // In degrees
    public float NearPlane { get; set; }
    public float FarPlane { get; set; }
    public bool IsPrimary { get; set; }
    
    public Matrix4x4 GetProjectionMatrix(float aspectRatio);
}
```

**Example:**
```csharp
var camera = registry.AddComponent<CameraComponent>(entity);
camera.FieldOfView = 60f;
camera.NearPlane = 0.1f;
camera.FarPlane = 1000f;
camera.IsPrimary = true;  // Make this the active camera

// Get projection matrix
var projMatrix = camera.GetProjectionMatrix(16f / 9f);
```

### InventoryComponent

Inventory system integration for entities.

```csharp
public class InventoryComponent : Component
{
    public Inventory Inventory { get; }
}
```

**Example:**
```csharp
// Player with inventory
var player = scene.CreateEntity("Player");
var inventory = registry.AddComponent<InventoryComponent>(player);
inventory.Inventory.AddItem(sword, 1);
inventory.Inventory.AddItem(potion, 5);
```

## Creating Custom Components

### Simple Component

```csharp
using EvokerEngine.ECS;

public class HealthComponent : Component
{
    public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; set; } = 100f;
    
    public bool IsAlive => CurrentHealth > 0f;
    
    public void TakeDamage(float damage)
    {
        CurrentHealth = Math.Max(0f, CurrentHealth - damage);
    }
    
    public void Heal(float amount)
    {
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
    }
}
```

**Usage:**
```csharp
var health = registry.AddComponent<HealthComponent>(enemy);
health.MaxHealth = 200f;
health.CurrentHealth = 200f;

health.TakeDamage(50f);
if (health.IsAlive)
{
    Logger.Info($"Enemy health: {health.CurrentHealth}");
}
```

### Component with Complex Data

```csharp
public class VelocityComponent : Component
{
    public Vector3 Linear { get; set; } = Vector3.Zero;
    public Vector3 Angular { get; set; } = Vector3.Zero;
    public float Drag { get; set; } = 0.1f;
    
    public void ApplyForce(Vector3 force, float mass)
    {
        Linear += force / mass;
    }
}
```

### Component with References

```csharp
public class ParentComponent : Component
{
    public Entity Parent { get; set; }
    public List<Entity> Children { get; set; } = new();
    
    public void AddChild(Entity child)
    {
        if (!Children.Contains(child))
        {
            Children.Add(child);
        }
    }
    
    public void RemoveChild(Entity child)
    {
        Children.Remove(child);
    }
}
```

## Component Operations

### Adding Components

```csharp
var transform = registry.AddComponent<TransformComponent>(entity);
var health = registry.AddComponent<HealthComponent>(entity);
var velocity = registry.AddComponent<VelocityComponent>(entity);
```

### Getting Components

```csharp
// Get component (returns null if not found)
var transform = registry.GetComponent<TransformComponent>(entity);
if (transform != null)
{
    transform.Position += Vector3.UnitY;
}

// Check if entity has component
if (registry.HasComponent<HealthComponent>(entity))
{
    var health = registry.GetComponent<HealthComponent>(entity);
    health.TakeDamage(10f);
}
```

### Removing Components

```csharp
// Remove a specific component
registry.RemoveComponent<VelocityComponent>(entity);

// Remove all components by destroying entity
registry.DestroyEntity(entity);
```

## Querying Components

### Get All Components of a Type

```csharp
// Get all health components
var allHealth = registry.GetAllComponents<HealthComponent>();
foreach (var health in allHealth)
{
    Logger.Info($"Entity {health.Entity.Id} has {health.CurrentHealth} HP");
}
```

### Get All Entities

```csharp
var allEntities = registry.GetAllEntities();
foreach (var entity in allEntities)
{
    Logger.Info($"Entity: {entity.Id}");
}
```

## System Pattern

While Evoker Engine doesn't enforce a specific system architecture, here's a recommended pattern:

### Example: Movement System

```csharp
public class MovementSystem
{
    private readonly ECSRegistry _registry;

    public MovementSystem(ECSRegistry registry)
    {
        _registry = registry;
    }

    public void Update(float deltaTime)
    {
        // Process all entities with Transform and Velocity
        var velocities = _registry.GetAllComponents<VelocityComponent>();
        
        foreach (var velocity in velocities)
        {
            var transform = _registry.GetComponent<TransformComponent>(velocity.Entity);
            if (transform != null)
            {
                // Apply velocity
                transform.Position += velocity.Linear * deltaTime;
                transform.Rotation += velocity.Angular * deltaTime;
                
                // Apply drag
                velocity.Linear *= (1f - velocity.Drag * deltaTime);
                velocity.Angular *= (1f - velocity.Drag * deltaTime);
            }
        }
    }
}
```

### Example: Health System

```csharp
public class HealthSystem
{
    private readonly ECSRegistry _registry;

    public HealthSystem(ECSRegistry registry)
    {
        _registry = registry;
    }

    public void Update(float deltaTime)
    {
        var allHealth = _registry.GetAllComponents<HealthComponent>();
        var entitiesToDestroy = new List<Entity>();
        
        foreach (var health in allHealth)
        {
            if (!health.IsAlive)
            {
                // Mark for destruction
                entitiesToDestroy.Add(health.Entity);
                OnEntityDied(health.Entity);
            }
        }
        
        // Clean up dead entities
        foreach (var entity in entitiesToDestroy)
        {
            _registry.DestroyEntity(entity);
        }
    }

    private void OnEntityDied(Entity entity)
    {
        Logger.Info($"Entity {entity.Id} died");
        // Spawn death effects, drop loot, etc.
    }
}
```

### Example: Rendering System

```csharp
public class RenderingSystem
{
    private readonly ECSRegistry _registry;

    public RenderingSystem(ECSRegistry registry)
    {
        _registry = registry;
    }

    public void Render()
    {
        // Get primary camera
        CameraComponent? primaryCamera = null;
        TransformComponent? cameraTransform = null;
        
        var cameras = _registry.GetAllComponents<CameraComponent>();
        foreach (var camera in cameras)
        {
            if (camera.IsPrimary)
            {
                primaryCamera = camera;
                cameraTransform = _registry.GetComponent<TransformComponent>(camera.Entity);
                break;
            }
        }
        
        if (primaryCamera == null || cameraTransform == null)
            return;
        
        // Render all mesh renderers
        var renderers = _registry.GetAllComponents<MeshRendererComponent>();
        foreach (var renderer in renderers)
        {
            var transform = _registry.GetComponent<TransformComponent>(renderer.Entity);
            if (transform != null)
            {
                RenderMesh(renderer, transform, primaryCamera, cameraTransform);
            }
        }
    }

    private void RenderMesh(MeshRendererComponent renderer, 
                           TransformComponent transform,
                           CameraComponent camera,
                           TransformComponent cameraTransform)
    {
        // Rendering implementation
    }
}
```

## Complete Example

```csharp
using EvokerEngine.Core;
using EvokerEngine.ECS;
using System.Numerics;

public class GameLayer : Layer
{
    private MovementSystem _movementSystem;
    private HealthSystem _healthSystem;
    private List<Entity> _enemies = new();

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;
        var registry = scene.Registry;
        
        // Initialize systems
        _movementSystem = new MovementSystem(registry);
        _healthSystem = new HealthSystem(registry);
        
        // Create player
        var player = scene.CreateEntity("Player");
        var playerTransform = registry.AddComponent<TransformComponent>(player);
        playerTransform.Position = new Vector3(0, 0, 0);
        
        var playerHealth = registry.AddComponent<HealthComponent>(player);
        playerHealth.MaxHealth = 100f;
        playerHealth.CurrentHealth = 100f;
        
        var playerVelocity = registry.AddComponent<VelocityComponent>(player);
        
        // Create enemies
        for (int i = 0; i < 5; i++)
        {
            var enemy = scene.CreateEntity($"Enemy_{i}");
            var enemyTransform = registry.AddComponent<TransformComponent>(enemy);
            enemyTransform.Position = new Vector3(i * 5, 0, 10);
            
            var enemyHealth = registry.AddComponent<HealthComponent>(enemy);
            enemyHealth.MaxHealth = 50f;
            enemyHealth.CurrentHealth = 50f;
            
            _enemies.Add(enemy);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        // Update systems
        _movementSystem.Update(deltaTime);
        _healthSystem.Update(deltaTime);
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            var scene = Application.Instance.ActiveScene;
            var registry = scene.Registry;
            
            // Find player
            var allHealth = registry.GetAllComponents<HealthComponent>();
            foreach (var health in allHealth)
            {
                if (health.MaxHealth == 100f) // Simple player identification
                {
                    if (keyEvent.KeyCode == Key.Number1)
                    {
                        health.TakeDamage(10f);
                    }
                }
            }
        }
    }
}
```

## Best Practices

### ✅ Do's

- Keep components as pure data containers
- Put logic in systems, not components
- Use composition over inheritance
- Query components efficiently
- Clean up destroyed entities

### ❌ Don'ts

- Don't store references to other registries in components
- Don't perform heavy computation in component properties
- Don't forget to null-check GetComponent() results
- Don't create circular component dependencies

## Performance Tips

1. **Cache component queries** - Don't query every frame if possible
2. **Batch operations** - Process similar entities together
3. **Use component pools** - Reuse components when possible
4. **Minimize component size** - Keep components small and focused

```csharp
// Bad: Query every frame
public override void OnUpdate(float deltaTime)
{
    var health = registry.GetComponent<HealthComponent>(_player);
}

// Good: Cache the component
private HealthComponent _playerHealth;

public override void OnAttach()
{
    _playerHealth = registry.GetComponent<HealthComponent>(_player);
}

public override void OnUpdate(float deltaTime)
{
    // Use cached reference
    if (_playerHealth != null)
    {
        // ...
    }
}
```

## See Also

- [Scene Management](../game-dev/scene-management.md) - Scene and entity creation
- [Application](application.md) - Application lifecycle
- [Layers](layers.md) - Layer system
