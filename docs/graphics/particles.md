# Particle System

The Evoker Engine includes a comprehensive particle system for creating visual effects like explosions, smoke, fire, magic spells, and environmental effects.

## Overview

The particle system consists of several components:

- **Particle**: Individual particle with position, velocity, size, color, rotation, and lifetime
- **ParticleEmitter**: Controls how particles are spawned
- **ParticleSystem**: Manages and simulates all particles
- **ParticleSystemComponent**: ECS component for attaching particle systems to entities
- **ParticleSystemManager**: Manages particle systems across entities

## Quick Start

### Creating a Particle System

```csharp
using EvokerEngine.Rendering;
using EvokerEngine.ECS;
using System.Numerics;

// Create an entity with a particle system component
var scene = Application.Instance.ActiveScene;
var entity = scene.CreateEntity("Fire Effect");

// Add transform
var transform = scene.Registry.AddComponent<TransformComponent>(entity);
transform.Position = new Vector3(0, 0, 0);

// Add particle system component
var particleComponent = scene.Registry.AddComponent<ParticleSystemComponent>(entity);

// Configure the particle system
particleComponent.MaxParticles = 500;
particleComponent.EmissionRate = 100f; // 100 particles per second
particleComponent.Lifetime = 2f; // Each particle lives for 2 seconds
particleComponent.StartSize = 0.5f;
particleComponent.EndSize = 0.1f;
particleComponent.StartColor = new Vector4(1, 0.5f, 0, 1); // Orange
particleComponent.EndColor = new Vector4(1, 0, 0, 0); // Fade to transparent red
particleComponent.Velocity = new Vector3(0, 5, 0); // Upward velocity
particleComponent.VelocityVariation = new Vector3(1, 1, 1); // Randomness
particleComponent.Gravity = new Vector3(0, -2f, 0); // Light upward drift
particleComponent.IsPlaying = true;
particleComponent.Loop = true;
```

## Particle System Components

### Particle

Individual particle data structure:

```csharp
public class Particle
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public float Size { get; set; }
    public Vector4 Color { get; set; }
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }
    public float Life { get; set; }
    public bool IsActive { get; set; }
}
```

### ParticleEmitter

Controls particle emission:

```csharp
var emitter = particleSystem.Emitter;

// Emission settings
emitter.EmissionRate = 200f; // Particles per second
emitter.Lifetime = 3f; // Particle lifetime
emitter.LifetimeVariation = 0.5f; // Random variation

// Size settings
emitter.StartSize = 1f;
emitter.EndSize = 0f;
emitter.SizeVariation = 0.2f; // Random variation

// Color settings
emitter.StartColor = new Vector4(1, 1, 1, 1); // White
emitter.EndColor = new Vector4(1, 1, 1, 0); // Transparent

// Velocity settings
emitter.Velocity = new Vector3(0, 10, 0);
emitter.VelocityVariation = new Vector3(2, 1, 2);

// Rotation settings
emitter.RotationRange = (0f, MathF.PI * 2f); // Random initial rotation
emitter.RotationSpeedRange = (-1f, 1f); // Rotation speed range
```

### Emission Shapes

Control where particles spawn:

```csharp
// Point emission (default)
emitter.Shape = EmissionShape.Point;

// Sphere emission
emitter.Shape = EmissionShape.Sphere;
emitter.ShapeRadius = 2f;

// Circle emission (flat disc)
emitter.Shape = EmissionShape.Circle;
emitter.ShapeRadius = 3f;

// Box emission
emitter.Shape = EmissionShape.Box;
emitter.BoxSize = new Vector3(2, 2, 2);

// Cone emission
emitter.Shape = EmissionShape.Cone;
emitter.ShapeRadius = 5f;
emitter.ShapeAngle = 30f; // Degrees
```

## Particle System Management

### Using ParticleSystemManager

```csharp
var manager = new ParticleSystemManager();

// Create particle system for an entity
var particleComponent = registry.GetComponent<ParticleSystemComponent>(entity);
var particleSystem = manager.CreateParticleSystem(entity, particleComponent);

// Update all particle systems (call every frame)
manager.Update(deltaTime, registry);

// Get particle system for an entity
var system = manager.GetParticleSystem(entity);

// Remove particle system
manager.RemoveParticleSystem(entity);
```

### Controlling Particle Systems

```csharp
// Play/pause
particleSystem.Play();
particleSystem.Pause();

// Stop and clear all particles
particleSystem.Stop();

// Clear particles but keep playing
particleSystem.Clear();

// Emit burst of particles
particleSystem.Burst(100, position);

// Check active particles
int count = particleSystem.ActiveParticleCount;
```

## Effect Examples

### Fire Effect

```csharp
var fire = new ParticleSystemComponent
{
    MaxParticles = 500,
    EmissionRate = 150f,
    Lifetime = 1.5f,
    StartSize = 0.8f,
    EndSize = 0.2f,
    StartColor = new Vector4(1, 0.8f, 0, 1), // Yellow-orange
    EndColor = new Vector4(1, 0, 0, 0), // Fade to red
    Velocity = new Vector3(0, 3, 0),
    VelocityVariation = new Vector3(0.5f, 1, 0.5f),
    Gravity = new Vector3(0, 1f, 0), // Rise
    IsPlaying = true,
    Loop = true
};
```

### Smoke Effect

```csharp
var smoke = new ParticleSystemComponent
{
    MaxParticles = 300,
    EmissionRate = 50f,
    Lifetime = 4f,
    StartSize = 0.5f,
    EndSize = 2f, // Grow over time
    StartColor = new Vector4(0.5f, 0.5f, 0.5f, 0.8f),
    EndColor = new Vector4(0.3f, 0.3f, 0.3f, 0),
    Velocity = new Vector3(0, 2, 0),
    VelocityVariation = new Vector3(1, 0.5f, 1),
    Gravity = new Vector3(0, 0.5f, 0),
    IsPlaying = true,
    Loop = true
};
```

### Explosion Effect

```csharp
var explosion = new ParticleSystemComponent
{
    MaxParticles = 200,
    EmissionRate = 1000f, // High rate for burst
    Lifetime = 2f,
    StartSize = 1f,
    EndSize = 0f,
    StartColor = new Vector4(1, 0.5f, 0, 1),
    EndColor = new Vector4(1, 0, 0, 0),
    Velocity = new Vector3(0, 0, 0),
    VelocityVariation = new Vector3(5, 5, 5), // Explode outward
    Gravity = new Vector3(0, -9.8f, 0),
    IsPlaying = false, // Don't loop
    Loop = false
};

// Trigger the explosion
var particleSystem = manager.GetParticleSystem(entity);
particleSystem.Burst(200, explosionPosition);
```

### Rain Effect

```csharp
var rain = new ParticleSystemComponent
{
    MaxParticles = 1000,
    EmissionRate = 500f,
    Lifetime = 3f,
    StartSize = 0.1f,
    EndSize = 0.1f, // Keep size
    StartColor = new Vector4(0.5f, 0.5f, 1, 0.5f),
    EndColor = new Vector4(0.5f, 0.5f, 1, 0.5f),
    Velocity = new Vector3(0, -20, 0), // Fall down
    VelocityVariation = new Vector3(2, 1, 2),
    Gravity = new Vector3(0, -5f, 0),
    IsPlaying = true,
    Loop = true
};

// Use box emission for area effect
var emitter = particleSystem.Emitter;
emitter.Shape = EmissionShape.Box;
emitter.BoxSize = new Vector3(50, 1, 50);
```

### Magic Spell Effect

```csharp
var magic = new ParticleSystemComponent
{
    MaxParticles = 300,
    EmissionRate = 100f,
    Lifetime = 2f,
    StartSize = 0.3f,
    EndSize = 0.1f,
    StartColor = new Vector4(0.5f, 0, 1, 1), // Purple
    EndColor = new Vector4(1, 0.5f, 1, 0), // Pink fade
    Velocity = new Vector3(0, 0, 0),
    VelocityVariation = new Vector3(2, 2, 2),
    Gravity = Vector3.Zero, // Float
    IsPlaying = true,
    Loop = true
};

// Use sphere emission
var emitter = particleSystem.Emitter;
emitter.Shape = EmissionShape.Sphere;
emitter.ShapeRadius = 1f;
emitter.RotationSpeedRange = (-2f, 2f); // Spinning particles
```

## Performance Considerations

1. **Particle Count**: Keep `MaxParticles` reasonable (1000-5000 for most effects)
2. **Emission Rate**: Higher rates create more particles but cost more performance
3. **Lifetime**: Longer lifetimes mean more active particles
4. **Multiple Systems**: You can have multiple particle systems for complex effects
5. **Culling**: Inactive particles don't cost much performance

## Advanced Usage

### Creating Custom Effects

```csharp
public class CustomParticleEffect
{
    private ParticleSystem _particleSystem;
    private float _timer;
    
    public void Initialize()
    {
        _particleSystem = new ParticleSystem
        {
            MaxParticles = 500,
            Gravity = new Vector3(0, -5, 0)
        };
        
        _particleSystem.Emitter.EmissionRate = 100f;
        _particleSystem.Initialize();
    }
    
    public void Update(float deltaTime, Vector3 position)
    {
        _timer += deltaTime;
        
        // Pulse effect - vary emission rate
        _particleSystem.Emitter.EmissionRate = 50f + 
            50f * MathF.Sin(_timer * 2f);
        
        // Update particle system
        _particleSystem.Update(deltaTime, position);
    }
}
```

### Combining Particle Systems

```csharp
// Create a complex effect with multiple systems
var fire = CreateFireSystem(entity);
var smoke = CreateSmokeSystem(entity);
var sparks = CreateSparksSystem(entity);

// Position them slightly differently
var fireTransform = registry.GetComponent<TransformComponent>(entity);
smokeTransform.Position = fireTransform.Position + new Vector3(0, 0.5f, 0);
sparksTransform.Position = fireTransform.Position;
```

## Integration with ECS

The particle system integrates seamlessly with the ECS:

```csharp
// In your game layer or system
public override void OnUpdate(float deltaTime)
{
    // Update particle systems
    _particleManager.Update(deltaTime, scene.Registry);
    
    // Access particles for custom behavior
    foreach (var entity in entitiesWithParticles)
    {
        var system = _particleManager.GetParticleSystem(entity);
        
        // Custom logic
        if (someCondition)
        {
            system.Burst(50, position);
        }
    }
}
```

## See Also

- [Rendering Pipeline](rendering-pipeline.md)
- [Entity Component System](../core/ecs.md)
- [3D Games Tutorial](../game-dev/3d-games.md)
