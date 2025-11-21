using System;
using System.Numerics;
using Xunit;
using EvokerEngine.Rendering;
using EvokerEngine.ECS;
using EvokerEngine.Scene;

namespace EvokerEngine.Tests;

public class ParticleSystemTests
{
    [Fact]
    public void Particle_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var particle = new Particle();

        // Assert
        Assert.False(particle.IsActive);
        Assert.Equal(Vector3.Zero, particle.Position);
        Assert.Equal(Vector3.Zero, particle.Velocity);
        Assert.Equal(0f, particle.Life);
    }

    [Fact]
    public void ParticleEmitter_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var emitter = new ParticleEmitter();

        // Assert
        Assert.Equal(100f, emitter.EmissionRate);
        Assert.Equal(1f, emitter.Lifetime);
        Assert.Equal(1f, emitter.StartSize);
        Assert.Equal(0f, emitter.EndSize);
        Assert.Equal(EmissionShape.Point, emitter.Shape);
    }

    [Fact]
    public void ParticleSystem_ShouldInitialize()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 100
        };

        // Act
        particleSystem.Initialize();

        // Assert
        Assert.Equal(100, particleSystem.Particles.Count);
        Assert.Equal(0, particleSystem.ActiveParticleCount);
        Assert.True(particleSystem.IsPlaying);
    }

    [Fact]
    public void ParticleSystem_Burst_ShouldActivateParticles()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 100
        };
        particleSystem.Initialize();

        // Act
        particleSystem.Burst(50, Vector3.Zero);

        // Assert
        Assert.Equal(50, particleSystem.ActiveParticleCount);
    }

    [Fact]
    public void ParticleSystem_Update_ShouldEmitParticles()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 100,
            IsPlaying = true
        };
        particleSystem.Emitter.EmissionRate = 100f;
        particleSystem.Initialize();

        // Act - Update for 0.5 seconds should emit ~50 particles
        particleSystem.Update(0.5f, Vector3.Zero);

        // Assert
        Assert.True(particleSystem.ActiveParticleCount > 0);
        Assert.True(particleSystem.ActiveParticleCount <= 100);
    }

    [Fact]
    public void ParticleSystem_Stop_ShouldClearAllParticles()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 100
        };
        particleSystem.Initialize();
        particleSystem.Burst(50, Vector3.Zero);
        Assert.Equal(50, particleSystem.ActiveParticleCount);

        // Act
        particleSystem.Stop();

        // Assert
        Assert.False(particleSystem.IsPlaying);
        Assert.Equal(0, particleSystem.ActiveParticleCount);
    }

    [Fact]
    public void ParticleSystem_Update_ShouldApplyGravity()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 10,
            Gravity = new Vector3(0, -10f, 0),
            IsPlaying = false
        };
        particleSystem.Emitter.Velocity = new Vector3(0, 5f, 0);
        particleSystem.Emitter.VelocityVariation = Vector3.Zero; // No variation
        particleSystem.Emitter.Lifetime = 10f; // Long lifetime so particle doesn't die
        particleSystem.Initialize();
        particleSystem.Burst(1, Vector3.Zero);
        
        var particle = particleSystem.Particles[0];
        var initialVelocityY = particle.Velocity.Y;

        // Act
        particleSystem.Update(1f, Vector3.Zero);

        // Assert - Velocity should have decreased in Y due to gravity
        Assert.True(particle.IsActive, "Particle should still be active");
        Assert.True(particle.Velocity.Y < initialVelocityY, 
            $"Expected velocity.Y ({particle.Velocity.Y}) to be less than initial ({initialVelocityY})");
    }

    [Fact]
    public void ParticleSystem_Update_ShouldKillParticlesWhenLifetimeExpires()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 10,
            IsPlaying = false // Don't emit new particles during update
        };
        particleSystem.Emitter.Lifetime = 0.1f;
        particleSystem.Emitter.LifetimeVariation = 0f; // No variation
        particleSystem.Initialize();
        particleSystem.Burst(5, Vector3.Zero);
        Assert.Equal(5, particleSystem.ActiveParticleCount);

        // Act - Update for longer than lifetime
        particleSystem.Update(0.2f, Vector3.Zero);

        // Assert
        Assert.Equal(0, particleSystem.ActiveParticleCount);
    }

    [Fact]
    public void ParticleEmitter_Burst_ShouldSetParticleProperties()
    {
        // Arrange
        var emitter = new ParticleEmitter
        {
            StartSize = 2f,
            EndSize = 0.5f,
            Lifetime = 5f,
            Velocity = new Vector3(0, 10, 0)
        };
        var particles = new System.Collections.Generic.List<Particle>();
        for (int i = 0; i < 10; i++)
        {
            particles.Add(new Particle { IsActive = false });
        }

        // Act
        emitter.Burst(particles, 5, Vector3.Zero);

        // Assert
        int activeCount = 0;
        foreach (var p in particles)
        {
            if (p.IsActive)
            {
                activeCount++;
                Assert.True(p.Life > 0);
                Assert.True(p.Size > 0);
            }
        }
        Assert.Equal(5, activeCount);
    }

    [Fact]
    public void ParticleSystemComponent_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var component = new ParticleSystemComponent();

        // Assert
        Assert.Equal(1000, component.MaxParticles);
        Assert.Equal(100f, component.EmissionRate);
        Assert.Equal(1f, component.Lifetime);
        Assert.Equal(1f, component.StartSize);
        Assert.Equal(0f, component.EndSize);
        Assert.True(component.IsPlaying);
        Assert.True(component.Loop);
    }

    [Fact]
    public void ParticleSystemManager_ShouldCreateParticleSystem()
    {
        // Arrange
        var manager = new ParticleSystemManager();
        var scene = new Scene.Scene("TestScene");
        var entity = scene.CreateEntity("ParticleEntity");
        var component = scene.Registry.AddComponent<ParticleSystemComponent>(entity);
        component.MaxParticles = 50;
        component.EmissionRate = 200f;

        // Act
        var particleSystem = manager.CreateParticleSystem(entity, component);

        // Assert
        Assert.NotNull(particleSystem);
        Assert.Equal(50, particleSystem.MaxParticles);
        Assert.Equal(200f, particleSystem.Emitter.EmissionRate);
    }

    [Fact]
    public void ParticleSystemManager_ShouldGetParticleSystem()
    {
        // Arrange
        var manager = new ParticleSystemManager();
        var scene = new Scene.Scene("TestScene");
        var entity = scene.CreateEntity("ParticleEntity");
        var component = scene.Registry.AddComponent<ParticleSystemComponent>(entity);
        
        var createdSystem = manager.CreateParticleSystem(entity, component);

        // Act
        var retrievedSystem = manager.GetParticleSystem(entity);

        // Assert
        Assert.NotNull(retrievedSystem);
        Assert.Same(createdSystem, retrievedSystem);
    }

    [Fact]
    public void ParticleSystemManager_ShouldRemoveParticleSystem()
    {
        // Arrange
        var manager = new ParticleSystemManager();
        var scene = new Scene.Scene("TestScene");
        var entity = scene.CreateEntity("ParticleEntity");
        var component = scene.Registry.AddComponent<ParticleSystemComponent>(entity);
        
        manager.CreateParticleSystem(entity, component);
        Assert.NotNull(manager.GetParticleSystem(entity));

        // Act
        manager.RemoveParticleSystem(entity);

        // Assert
        Assert.Null(manager.GetParticleSystem(entity));
    }

    [Fact]
    public void ParticleEmitter_DifferentShapes_ShouldEmitParticles()
    {
        // Arrange
        var shapes = new[] { EmissionShape.Point, EmissionShape.Sphere, EmissionShape.Circle, EmissionShape.Box, EmissionShape.Cone };
        
        foreach (var shape in shapes)
        {
            var emitter = new ParticleEmitter
            {
                Shape = shape,
                ShapeRadius = 2f
            };
            var particles = new System.Collections.Generic.List<Particle>();
            for (int i = 0; i < 10; i++)
            {
                particles.Add(new Particle { IsActive = false });
            }

            // Act
            emitter.Burst(particles, 5, Vector3.Zero);

            // Assert
            int activeCount = 0;
            foreach (var p in particles)
            {
                if (p.IsActive) activeCount++;
            }
            Assert.Equal(5, activeCount);
        }
    }

    [Fact]
    public void ParticleSystem_PlayPause_ShouldControlEmission()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 100
        };
        particleSystem.Initialize();
        particleSystem.Emitter.EmissionRate = 100f;

        // Act - Pause should stop emission
        particleSystem.Pause();
        particleSystem.Update(0.5f, Vector3.Zero);
        var countWhenPaused = particleSystem.ActiveParticleCount;

        // Play should resume emission
        particleSystem.Play();
        particleSystem.Update(0.5f, Vector3.Zero);
        var countWhenPlaying = particleSystem.ActiveParticleCount;

        // Assert
        Assert.Equal(0, countWhenPaused);
        Assert.True(countWhenPlaying > 0);
    }

    [Fact]
    public void ParticleSystem_Clear_ShouldDeactivateAllParticles()
    {
        // Arrange
        var particleSystem = new ParticleSystem
        {
            MaxParticles = 100
        };
        particleSystem.Initialize();
        particleSystem.Burst(50, Vector3.Zero);
        Assert.Equal(50, particleSystem.ActiveParticleCount);

        // Act
        particleSystem.Clear();

        // Assert
        Assert.Equal(0, particleSystem.ActiveParticleCount);
        Assert.True(particleSystem.IsPlaying); // Should still be playing, just cleared
    }
}
