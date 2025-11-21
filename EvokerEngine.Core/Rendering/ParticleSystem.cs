using System;
using System.Collections.Generic;
using System.Numerics;
using EvokerEngine.ECS;

namespace EvokerEngine.Rendering;

/// <summary>
/// Represents a single particle in the system
/// </summary>
public class Particle
{
    /// <summary>
    /// Current position of the particle
    /// </summary>
    public Vector3 Position { get; set; }
    
    /// <summary>
    /// Current velocity of the particle
    /// </summary>
    public Vector3 Velocity { get; set; }
    
    /// <summary>
    /// Current size of the particle
    /// </summary>
    public float Size { get; set; }
    
    /// <summary>
    /// Current color of the particle
    /// </summary>
    public Vector4 Color { get; set; }
    
    /// <summary>
    /// Current rotation in radians
    /// </summary>
    public float Rotation { get; set; }
    
    /// <summary>
    /// Rotation speed in radians per second
    /// </summary>
    public float RotationSpeed { get; set; }
    
    /// <summary>
    /// Remaining lifetime in seconds
    /// </summary>
    public float Life { get; set; }
    
    /// <summary>
    /// Initial lifetime for interpolation
    /// </summary>
    public float InitialLife { get; set; }
    
    /// <summary>
    /// Whether this particle is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Start size for lerping
    /// </summary>
    public float StartSize { get; set; }
    
    /// <summary>
    /// End size for lerping
    /// </summary>
    public float EndSize { get; set; }
    
    /// <summary>
    /// Start color for lerping
    /// </summary>
    public Vector4 StartColor { get; set; }
    
    /// <summary>
    /// End color for lerping
    /// </summary>
    public Vector4 EndColor { get; set; }
}

/// <summary>
/// Particle emitter that spawns particles
/// </summary>
public class ParticleEmitter
{
    private Random _random = new Random();
    private float _accumulatedTime = 0f;
    
    /// <summary>
    /// Emission rate (particles per second)
    /// </summary>
    public float EmissionRate { get; set; } = 100f;
    
    /// <summary>
    /// Base velocity for new particles
    /// </summary>
    public Vector3 Velocity { get; set; } = new Vector3(0, 5, 0);
    
    /// <summary>
    /// Velocity randomness
    /// </summary>
    public Vector3 VelocityVariation { get; set; } = new Vector3(1, 1, 1);
    
    /// <summary>
    /// Particle lifetime
    /// </summary>
    public float Lifetime { get; set; } = 1f;
    
    /// <summary>
    /// Lifetime randomness
    /// </summary>
    public float LifetimeVariation { get; set; } = 0.2f;
    
    /// <summary>
    /// Start size
    /// </summary>
    public float StartSize { get; set; } = 1f;
    
    /// <summary>
    /// End size
    /// </summary>
    public float EndSize { get; set; } = 0f;
    
    /// <summary>
    /// Size randomness
    /// </summary>
    public float SizeVariation { get; set; } = 0.2f;
    
    /// <summary>
    /// Start color
    /// </summary>
    public Vector4 StartColor { get; set; } = Vector4.One;
    
    /// <summary>
    /// End color
    /// </summary>
    public Vector4 EndColor { get; set; } = new Vector4(1, 1, 1, 0);
    
    /// <summary>
    /// Initial rotation range (min, max) in radians
    /// </summary>
    public (float min, float max) RotationRange { get; set; } = (0f, 0f);
    
    /// <summary>
    /// Rotation speed range (min, max) in radians per second
    /// </summary>
    public (float min, float max) RotationSpeedRange { get; set; } = (0f, 0f);
    
    /// <summary>
    /// Emission shape
    /// </summary>
    public EmissionShape Shape { get; set; } = EmissionShape.Point;
    
    /// <summary>
    /// Shape radius (for sphere, circle, cone)
    /// </summary>
    public float ShapeRadius { get; set; } = 1f;
    
    /// <summary>
    /// Shape angle for cone emission (in degrees)
    /// </summary>
    public float ShapeAngle { get; set; } = 30f;
    
    /// <summary>
    /// Box dimensions for box emission
    /// </summary>
    public Vector3 BoxSize { get; set; } = Vector3.One;
    
    /// <summary>
    /// Burst emission (emit all particles at once)
    /// </summary>
    public void Burst(List<Particle> particles, int count, Vector3 position)
    {
        for (int i = 0; i < count; i++)
        {
            var particle = GetInactiveParticle(particles);
            if (particle != null)
            {
                EmitParticle(particle, position);
            }
        }
    }
    
    /// <summary>
    /// Update emission and spawn particles
    /// </summary>
    public void Update(float deltaTime, List<Particle> particles, Vector3 position)
    {
        _accumulatedTime += deltaTime;
        
        float interval = 1f / EmissionRate;
        
        while (_accumulatedTime >= interval)
        {
            _accumulatedTime -= interval;
            
            var particle = GetInactiveParticle(particles);
            if (particle != null)
            {
                EmitParticle(particle, position);
            }
        }
    }
    
    private Particle? GetInactiveParticle(List<Particle> particles)
    {
        foreach (var p in particles)
        {
            if (!p.IsActive)
                return p;
        }
        return null;
    }
    
    private void EmitParticle(Particle particle, Vector3 position)
    {
        particle.IsActive = true;
        particle.Position = position + GetShapeOffset();
        particle.Velocity = GetVelocity();
        particle.Life = Lifetime + ((float)_random.NextDouble() * 2f - 1f) * LifetimeVariation;
        particle.InitialLife = particle.Life;
        
        float sizeVar = ((float)_random.NextDouble() * 2f - 1f) * SizeVariation;
        particle.StartSize = Math.Max(0.01f, StartSize + sizeVar);
        particle.EndSize = Math.Max(0f, EndSize + sizeVar);
        particle.Size = particle.StartSize;
        
        particle.StartColor = StartColor;
        particle.EndColor = EndColor;
        particle.Color = StartColor;
        
        particle.Rotation = Lerp(RotationRange.min, RotationRange.max, (float)_random.NextDouble());
        particle.RotationSpeed = Lerp(RotationSpeedRange.min, RotationSpeedRange.max, (float)_random.NextDouble());
    }
    
    private Vector3 GetShapeOffset()
    {
        switch (Shape)
        {
            case EmissionShape.Point:
                return Vector3.Zero;
                
            case EmissionShape.Sphere:
                return RandomInSphere() * ShapeRadius;
                
            case EmissionShape.Circle:
                var circle = RandomInCircle() * ShapeRadius;
                return new Vector3(circle.X, 0, circle.Y);
                
            case EmissionShape.Box:
                return new Vector3(
                    ((float)_random.NextDouble() * 2f - 1f) * BoxSize.X * 0.5f,
                    ((float)_random.NextDouble() * 2f - 1f) * BoxSize.Y * 0.5f,
                    ((float)_random.NextDouble() * 2f - 1f) * BoxSize.Z * 0.5f
                );
                
            case EmissionShape.Cone:
                return RandomInCone();
                
            default:
                return Vector3.Zero;
        }
    }
    
    private Vector3 GetVelocity()
    {
        Vector3 vel = Velocity;
        vel.X += ((float)_random.NextDouble() * 2f - 1f) * VelocityVariation.X;
        vel.Y += ((float)_random.NextDouble() * 2f - 1f) * VelocityVariation.Y;
        vel.Z += ((float)_random.NextDouble() * 2f - 1f) * VelocityVariation.Z;
        return vel;
    }
    
    private Vector3 RandomInSphere()
    {
        float theta = (float)_random.NextDouble() * MathF.PI * 2f;
        float phi = MathF.Acos(2f * (float)_random.NextDouble() - 1f);
        float r = MathF.Pow((float)_random.NextDouble(), 1f / 3f);
        
        return new Vector3(
            r * MathF.Sin(phi) * MathF.Cos(theta),
            r * MathF.Sin(phi) * MathF.Sin(theta),
            r * MathF.Cos(phi)
        );
    }
    
    private Vector2 RandomInCircle()
    {
        float angle = (float)_random.NextDouble() * MathF.PI * 2f;
        float r = MathF.Sqrt((float)_random.NextDouble());
        return new Vector2(r * MathF.Cos(angle), r * MathF.Sin(angle));
    }
    
    private Vector3 RandomInCone()
    {
        float angle = (float)_random.NextDouble() * MathF.PI * 2f;
        float maxAngleRad = ShapeAngle * MathF.PI / 180f;
        float coneAngle = (float)_random.NextDouble() * maxAngleRad;
        float distance = (float)_random.NextDouble() * ShapeRadius;
        
        float x = distance * MathF.Sin(coneAngle) * MathF.Cos(angle);
        float y = distance * MathF.Cos(coneAngle);
        float z = distance * MathF.Sin(coneAngle) * MathF.Sin(angle);
        
        return new Vector3(x, y, z);
    }
    
    private float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}

/// <summary>
/// Emission shapes for particle emitters
/// </summary>
public enum EmissionShape
{
    Point,
    Sphere,
    Circle,
    Box,
    Cone
}

/// <summary>
/// Particle system that manages and simulates particles
/// </summary>
public class ParticleSystem
{
    private List<Particle> _particles = new List<Particle>();
    private ParticleEmitter _emitter = new ParticleEmitter();
    
    /// <summary>
    /// Maximum number of particles
    /// </summary>
    public int MaxParticles { get; set; } = 1000;
    
    /// <summary>
    /// Whether the system is playing
    /// </summary>
    public bool IsPlaying { get; set; } = true;
    
    /// <summary>
    /// Whether to loop the emission
    /// </summary>
    public bool Loop { get; set; } = true;
    
    /// <summary>
    /// Gravity affecting particles
    /// </summary>
    public Vector3 Gravity { get; set; } = new Vector3(0, -9.8f, 0);
    
    /// <summary>
    /// Get the particle emitter
    /// </summary>
    public ParticleEmitter Emitter => _emitter;
    
    /// <summary>
    /// Get all particles
    /// </summary>
    public IReadOnlyList<Particle> Particles => _particles;
    
    /// <summary>
    /// Get count of active particles
    /// </summary>
    public int ActiveParticleCount
    {
        get
        {
            int count = 0;
            foreach (var p in _particles)
            {
                if (p.IsActive) count++;
            }
            return count;
        }
    }
    
    /// <summary>
    /// Initialize the particle system
    /// </summary>
    public void Initialize()
    {
        _particles.Clear();
        for (int i = 0; i < MaxParticles; i++)
        {
            _particles.Add(new Particle { IsActive = false });
        }
    }
    
    /// <summary>
    /// Update all particles
    /// </summary>
    public void Update(float deltaTime, Vector3 emitterPosition)
    {
        // Emit new particles
        if (IsPlaying)
        {
            _emitter.Update(deltaTime, _particles, emitterPosition);
        }
        
        // Update existing particles
        for (int i = 0; i < _particles.Count; i++)
        {
            var particle = _particles[i];
            if (!particle.IsActive)
                continue;
                
            // Update lifetime
            particle.Life -= deltaTime;
            if (particle.Life <= 0f)
            {
                particle.IsActive = false;
                continue;
            }
            
            // Calculate life fraction
            float lifeFraction = 1f - (particle.Life / particle.InitialLife);
            
            // Update physics
            particle.Velocity += Gravity * deltaTime;
            particle.Position += particle.Velocity * deltaTime;
            
            // Update rotation
            particle.Rotation += particle.RotationSpeed * deltaTime;
            
            // Interpolate size
            particle.Size = Lerp(particle.StartSize, particle.EndSize, lifeFraction);
            
            // Interpolate color
            particle.Color = LerpColor(particle.StartColor, particle.EndColor, lifeFraction);
        }
    }
    
    /// <summary>
    /// Emit a burst of particles
    /// </summary>
    public void Burst(int count, Vector3 position)
    {
        _emitter.Burst(_particles, count, position);
    }
    
    /// <summary>
    /// Play the particle system
    /// </summary>
    public void Play()
    {
        IsPlaying = true;
    }
    
    /// <summary>
    /// Pause the particle system
    /// </summary>
    public void Pause()
    {
        IsPlaying = false;
    }
    
    /// <summary>
    /// Stop and clear all particles
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
        foreach (var particle in _particles)
        {
            particle.IsActive = false;
        }
    }
    
    /// <summary>
    /// Clear all particles immediately
    /// </summary>
    public void Clear()
    {
        foreach (var particle in _particles)
        {
            particle.IsActive = false;
        }
    }
    
    private float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    
    private Vector4 LerpColor(Vector4 a, Vector4 b, float t)
    {
        return new Vector4(
            Lerp(a.X, b.X, t),
            Lerp(a.Y, b.Y, t),
            Lerp(a.Z, b.Z, t),
            Lerp(a.W, b.W, t)
        );
    }
}

/// <summary>
/// System that manages particle systems attached to entities
/// </summary>
public class ParticleSystemManager
{
    private Dictionary<Entity, ParticleSystem> _particleSystems = new Dictionary<Entity, ParticleSystem>();
    
    /// <summary>
    /// Create a particle system for an entity
    /// </summary>
    public ParticleSystem CreateParticleSystem(Entity entity, ParticleSystemComponent component)
    {
        var particleSystem = new ParticleSystem
        {
            MaxParticles = component.MaxParticles,
            IsPlaying = component.IsPlaying,
            Loop = component.Loop,
            Gravity = component.Gravity
        };
        
        // Configure emitter
        particleSystem.Emitter.EmissionRate = component.EmissionRate;
        particleSystem.Emitter.Lifetime = component.Lifetime;
        particleSystem.Emitter.StartSize = component.StartSize;
        particleSystem.Emitter.EndSize = component.EndSize;
        particleSystem.Emitter.StartColor = component.StartColor;
        particleSystem.Emitter.EndColor = component.EndColor;
        particleSystem.Emitter.Velocity = component.Velocity;
        particleSystem.Emitter.VelocityVariation = component.VelocityVariation;
        
        particleSystem.Initialize();
        
        _particleSystems[entity] = particleSystem;
        return particleSystem;
    }
    
    /// <summary>
    /// Get particle system for an entity
    /// </summary>
    public ParticleSystem? GetParticleSystem(Entity entity)
    {
        _particleSystems.TryGetValue(entity, out var system);
        return system;
    }
    
    /// <summary>
    /// Remove particle system for an entity
    /// </summary>
    public void RemoveParticleSystem(Entity entity)
    {
        _particleSystems.Remove(entity);
    }
    
    /// <summary>
    /// Update all particle systems
    /// </summary>
    public void Update(float deltaTime, ECSRegistry registry)
    {
        var entities = new List<Entity>(_particleSystems.Keys);
        
        foreach (var entity in entities)
        {
            var transform = registry.GetComponent<TransformComponent>(entity);
            if (transform == null)
                continue;
                
            var particleSystem = _particleSystems[entity];
            particleSystem.Update(deltaTime, transform.Position);
        }
    }
    
    /// <summary>
    /// Clear all particle systems
    /// </summary>
    public void Clear()
    {
        _particleSystems.Clear();
    }
}
